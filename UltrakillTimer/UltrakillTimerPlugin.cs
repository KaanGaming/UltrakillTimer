using BepInEx;
using RoR2;
using Mono.Cecil;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using R2API.Utils;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using MonoMod.RuntimeDetour;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using System.Collections;
using UltrakillTimer.Utils;
using BepInEx.Configuration;
using UnityEngine.UI;

namespace UltrakillTimer
{
	[NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
	[BepInDependency("com.rune580.riskofoptions")]
	[BepInPlugin("com.kanggamming.ultrakilltimer", "ULTRAKILL Timer", "0.1.0")]
    public class UltrakillTimerPlugin
		: BaseUnityPlugin
    {
		public UltrakillTimerPlugin()
			: base()
		{

		}

		private AssetBundle _ab;
		private AssetBundle _abm;
		private GameObject _timer;
		private GameObject _currenttimer;
		private Run.FixedTimeStamp _endtime;

		#region config
		// General
		private static ConfigEntry<bool> LoadMusicBundle { get; set; }

		// Visual
		private static ConfigEntry<bool> OverridePulseBPM { get; set; }
		private static ConfigEntry<int> CustomPulseBPM { get; set; }
		private static ConfigEntry<float> BackgroundAlpha { get; set; }
		private static ConfigEntry<Color> NormalColor { get; set; }
		private static ConfigEntry<Color> FlashColor { get; set; }

		// Music
		private static ConfigEntry<float> MusicVolume { get; set; }
		private static ConfigEntry<bool> IgnoreInGameMusicVolume { get; set; }

		// Misc
		private static ConfigEntry<float> TimerTimePreview { get; set; }
		#endregion

		internal static float MusicVolumeConfig => MusicVolume.Value;

		internal static bool IgnoreIGVolume => IgnoreInGameMusicVolume.Value;

		public void Awake()
		{
			_log = Logger;

			#region config setup
			LoadMusicBundle = Config.Bind<bool>("General", "LoadMusicBundle", true, "Whether to load the music bundle. This is required to play the music from 7-4 in ULTRAKILL while escaping the moon.");

			OverridePulseBPM = Config.Bind<bool>("Visual", "OverridePulseBPM", false, "Whether to override the BPM of the pulsing animation for the timer. The timer pulses every 1/2 beat, and the default BPM is 120 BPM, so 120 BPM = Pulse every 1 second");
			CustomPulseBPM = Config.Bind<int>("Visual", "CustomPulseBPM", 120, "Refer to the description of Override Pulse BPM");
			BackgroundAlpha = Config.Bind<float>("Visual", "BackgroundAlpha", 170, "The alpha value/opacity of the background panel of the timer. Default is 220, min is 0, max is 255");
			NormalColor = Config.Bind<Color>("Visual", "NormalColor", new Color(1, 0, 0, 1), "The normal color the timer uses.");
			FlashColor = Config.Bind<Color>("Visual", "FlashColor", new Color(1, 1, 0, 1), "The color the timer pulses.");

			MusicVolume = Config.Bind<float>("Music", "MusicVolume", 1f, "The volume of the music. The music is already affected by the in-game volume sliders, but feel free to adjust the volume of the escape music with this as well!");
			IgnoreInGameMusicVolume = Config.Bind<bool>("Music", "IgnoreInGameMusicVolume", false, "If true, ignores the in-game volume.");
			TimerTimePreview = Config.Bind<float>("Misc", "TimerTimePreview", 80f, "Used for in-game settings menu made with Risk of Options, please ignore");
			#endregion

			#region risk of options setup
			ModSettingsManager.SetModDescription("A plugin that replaces the in-game timer for the moon escape sequence with the timer from 7-4 in ULTRAKILL");

			ModSettingsManager.AddOption(new CheckBoxOption(LoadMusicBundle, new CheckBoxConfig
			{
				category = "General",
				description = "Whether to load the music bundle. This is required to play the music from 7-4 in ULTRAKILL while escaping the moon. Only affects startup, so restarting the game is required to toggle this option.\n\n(This option is really pointless as of now, but maybe it'll have a use in a future update.)",
				restartRequired = true,
				name = "Load music bundle on start"
			}));

			ModSettingsManager.AddOption(new CheckBoxOption(OverridePulseBPM, new CheckBoxConfig
			{
				category = "Visual",
				description = "Overrides the default BPM of the pulse animation for the timer.",
				restartRequired = false,
				name = "Override pulse BPM"
			}));

			ModSettingsManager.AddOption(new IntSliderOption(CustomPulseBPM, new IntSliderConfig
			{
				category = "Visual",
				description = "Changes the BPM of the pulse animation if \"Override pulse BPM\" is enabled. The timer pulses every 1/2 beat, and the default BPM is 120 BPM, so with 120 BPM the timer would pulse every second.",
				restartRequired = false,
				min = 1,
				max = 1000,
				name = "Custom BPM"
			}));

			ModSettingsManager.AddOption(new StepSliderOption(BackgroundAlpha, new StepSliderConfig
			{
				category = "Visual",
				description = "The alpha value/opacity of the background color on the timer. Default is 220, min is 0 and max is 255.",
				min = 0,
				max = 255,
				increment = 1,
				formatString = "{0:G}",
				name = "Background opacity"
			}));

			ModSettingsManager.AddOption(new ColorOption(NormalColor, new ColorOptionConfig
			{
				category = "Visual",
				description = "The color of the timer. The default value is purely red.",
				restartRequired = false,
				name = "Timer color"
			}));

			ModSettingsManager.AddOption(new ColorOption(FlashColor, new ColorOptionConfig
			{
				category = "Visual",
				description = "The color the timer pulses. The default value is purely yellow.",
				restartRequired = false,
				name = "Timer flash color"
			}));

			ModSettingsManager.AddOption(new SliderOption(MusicVolume, new SliderConfig
			{
				category = "Music",
				description = "The volume of the music. The music is already affected by the in-game volume sliders, but feel free to adjust the volume of the escape music with this as well! (1.00 is 100%, default is 1.00)",
				restartRequired = false,
				min = 0,
				formatString = "{0:G}",
				max = 3f,
				name = "Escape music volume"
			}));

			ModSettingsManager.AddOption(new CheckBoxOption(IgnoreInGameMusicVolume, new CheckBoxConfig
			{
				category = "Music",
				description = "Ignore the value of the music volume found in the Audio page in the actual Risk of Rain 2 settings page.",
				restartRequired = false,
				name = "Ignore in-game music volume"
			}));

			ModSettingsManager.AddOption(new GenericButtonOption("Preview Music 1", "Preview", "Listen to the first phase of the escape music, played right after the moon detonation becomes imminent and before you enter one of the portals to escape the arena.", "Play", TestMusic1));

			ModSettingsManager.AddOption(new GenericButtonOption("Preview Music 2", "Preview", "Listen to the second phase of the escape music, played right after you touch one of the portals that lead you out of the arena.", "Play", TestMusic2));

			ModSettingsManager.AddOption(new GenericButtonOption("Stop Music Preview", "Preview", "Stop previewing the custom escape music.", "Stop", StopAllMusic));

			ModSettingsManager.AddOption(new StepSliderOption(TimerTimePreview, new StepSliderConfig
			{
				category = "Preview",
				description = "Amount of seconds that the timer will start ticking down from. Used only in the preview of the timer.",
				name = "Timer Preview's Time",
				min = 0f,
				max = 600f,
				increment = 1f,
			}));

			ModSettingsManager.AddOption(new GenericButtonOption("Preview Timer", "Preview", "Spawn the timer at the top and play the music as well.\n\n(NOTE: If you want to edit visual settings while previewing, you need to click on this button again to see changes)", "Preview", PreviewTimerWithMusic));

			ModSettingsManager.AddOption(new GenericButtonOption("Preview Timer w/o Music", "Preview", "Spawn the timer at the top but don't play the music.\n\n(NOTE: If you want to edit visual settings while previewing, you need to click on this button again to see changes)", "Preview", PreviewTimer));

			ModSettingsManager.AddOption(new GenericButtonOption("Destroy Timer Preview", "Preview", "Stops displaying the timer and playing the music.", "Destroy", KillTimer));
			#endregion

			Logger.LogInfo("Loading timer asset bundle");
			_ab = AssetBundle.LoadFromMemory(GetEmbeddedResource("ultrakilltimerbundle"));
			Logger.LogInfo("Loading timer prefab...");
			_timer = _ab.LoadAsset<GameObject>("assets/critical failure timer.prefab");
			Logger.LogInfo($"Loaded prefab {_timer.name} successfully.");

			Logger.LogInfo(LoadMusicBundle.Value ? "Loading escape music asset bundle" : "Skipping loading escape music asset bundle");
			if (LoadMusicBundle.Value)
			{
				_abm = AssetBundle.LoadFromMemory(GetEmbeddedResource("ultrakilltimersoundbundle"));
				Logger.LogInfo("Loading music...");
				MusicController.phase1 = _abm.LoadAsset<AudioClip>("assets/centaur b-4.ogg");
				Logger.LogInfo("Phase 1 music loaded");
				MusicController.phase2 = _abm.LoadAsset<AudioClip>("assets/centaur b-5.ogg");
				Logger.LogInfo("Phase 2 music loaded");
				Logger.LogInfo("Loaded escape music successfully.");
			}

			On.RoR2.EscapeSequenceController.SetHudCountdownEnabled += SetHUDCountdownEnabled;
			On.RoR2.EscapeSequenceController.EscapeSequenceMainState.OnEnter += EscapeSeqMainStateOnEnter;
			PhaseChangeCollisionCheck.OnCollisionEnterEvent += OnEnterArenaPortal;
		}

		#region logging stuff
		internal static ManualLogSource _log;

		internal static void LogInfo(string msg)
		{
			_log.LogInfo(msg);
		}

		internal static void LogError(string msg)
		{
			_log.LogError(msg);
		}

		internal static void LogWarning(string msg)
		{
			_log.LogWarning(msg);
		}

		internal static void LogDebug(string msg)
		{
			_log.LogDebug(msg);
		}
		#endregion

		private void TestMusic1()
		{
			MusicController.PlayPhase(1);
		}

		private void TestMusic2()
		{
			MusicController.PlayPhase(2);
		}

		private void TestMusic2IfStillPlaying()
		{
			if (_preview == true)
				MusicController.PlayPhase(2);
		}

		private void StopAllMusic()
		{
			MusicController.Stop();
			MusicController.DestroyMusicController();
		}

		private float _previewTime;
		private GameObject _previewTimer;
		private bool _preview = false;

		private void PreviewTimerWithMusic()
		{
			_preview = true;

			IEnumerator delaySwitch()
			{
				yield return new WaitForSeconds(16f);
				TestMusic2IfStillPlaying();
			}

			if (_previewTimer != null)
				DestroyTimer(_previewTimer);
			
			TestMusic1();
			CoroutineRunner.RunCoroutine(delaySwitch(), 18f);
			_previewTime = TimerTimePreview.Value;
			_previewTimer = CreateTimer(_previewTime);
		}

		private void PreviewTimer()
		{
			if (_previewTimer != null)
				DestroyTimer(_previewTimer);

			_previewTime = TimerTimePreview.Value;
			_previewTimer = CreateTimer(_previewTime);
		}

		private void KillTimer()
		{
			DestroyTimer(_previewTimer);
			StopAllMusic();
			_preview = false;
		}

		public void Update()
		{
			//if (Input.GetKeyDown(KeyCode.P))
			//{
			//    CreateTimer(Run.FixedTimeStamp.now + 80f);
			//}
		}

		private void OnEnterArenaPortal(GameObject obj)
		{
			if (MusicController.InstanceExists() && MusicController.currentPhase != 2)
			{
				MusicController.PlayPhase(2);
			}
		}

		private void EscapeSeqMainStateOnEnter(On.RoR2.EscapeSequenceController.EscapeSequenceMainState.orig_OnEnter orig, EscapeSequenceController.EscapeSequenceMainState self)
		{
			orig(self);
			_endtime = self.GetFieldValue<Run.FixedTimeStamp>("endTime");
			MusicController.PlayPhase(1);
			CoroutineRunner.RunCoroutine(AttachPhaseChangeChecks(), 1f);
		}

		private IEnumerator AttachPhaseChangeChecks()
		{
			yield return new WaitForSeconds(0.25f);
			PhaseChangeCollisionCheck.AddComponentToPortals();
		}

		private void SetHUDCountdownEnabled(On.RoR2.EscapeSequenceController.orig_SetHudCountdownEnabled orig, EscapeSequenceController self, RoR2.UI.HUD hud, bool shouldEnableCountdownPanel)
		{
			if (_currenttimer != shouldEnableCountdownPanel)
			{
				if (shouldEnableCountdownPanel)
				{
					_currenttimer = CreateTimer(_endtime);
				}
				else
				{
					DestroyTimer(_currenttimer);
				}
			}
		}

		private void DestroyTimer(GameObject timer)
		{
			GameObject parentcanvas = timer.transform.parent.gameObject;
			GameObject.Destroy(timer);
			GameObject.Destroy(parentcanvas);
		}

		private GameObject CreateTimer(Run.FixedTimeStamp endTime)
		{
			GameObject canvasobj = GameObject.Find("Timer Canvas");
			Canvas canvas;
			if (canvasobj == null)
			{
				canvasobj = new GameObject("Timer Canvas");
				canvas = canvasobj.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.referencePixelsPerUnit = 100;
				canvas.scaleFactor = 1;
			}
			else
			{
				canvas = canvasobj.GetComponent<Canvas>();
			}

			var timer = GameObject.Instantiate(_timer, canvas.transform);
			var bgimage = timer.transform.GetChild(0).GetComponent<Image>();
			bgimage.color = new Color(bgimage.color.r, bgimage.color.g, bgimage.color.b, BackgroundAlpha.Value / 255f);
			var ctrl = timer.AddComponent<TimerController>();
			ctrl.delay = 0;
			ctrl.fadeTime = OverridePulseBPM.Value ? 120 / CustomPulseBPM.Value : 1;
			ctrl.flashColor = FlashColor.Value;
			ctrl.originalColor = NormalColor.Value;
			ctrl.timerEnd = endTime;

			Logger.LogInfo($"Timer created with {endTime.timeUntilClamped}s, fadeTime = {ctrl.fadeTime}");

			return timer;
		}

		private GameObject CreateTimer(float endTime)
		{
			GameObject canvasobj = GameObject.Find("Timer Canvas");
			Canvas canvas;
			if (canvasobj == null)
			{
				canvasobj = new GameObject("Timer Canvas");
				canvas = canvasobj.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.referencePixelsPerUnit = 100;
				canvas.scaleFactor = 1;
			}
			else
			{
				canvas = canvasobj.GetComponent<Canvas>();
			}

			var timer = GameObject.Instantiate(_timer, canvas.transform);
			var bgimage = timer.transform.GetChild(0).GetComponent<Image>();
			bgimage.color = new Color(bgimage.color.r, bgimage.color.g, bgimage.color.b, BackgroundAlpha.Value / 255f);
			var ctrl = timer.AddComponent<TimerController>();
			ctrl.delay = 0;
			ctrl.fadeTime = OverridePulseBPM.Value ? 120f / CustomPulseBPM.Value : 1;
			ctrl.flashColor = FlashColor.Value;
			ctrl.originalColor = NormalColor.Value;
			ctrl.useCustomTimerEnd = true;
			ctrl.customTimerEnd = endTime;

			Logger.LogInfo($"Timer created with {endTime}s, fadeTime = {ctrl.fadeTime}");

			return timer;
		}

		private byte[] GetEmbeddedResource(string name)
		{
			Logger.LogInfo($"Getting resource {name}");
			Assembly asm = Assembly.GetExecutingAssembly();
			byte[] output = null;
			string resourceName = null;

			foreach (string str in asm.GetManifestResourceNames())
			{
				if (str.Contains(name))
					resourceName = str;
			}
			
			if (string.IsNullOrEmpty(resourceName))
			{
				Logger.LogError($"Resource {resourceName} not found");
				throw new ArgumentException("Embedded resource not found", nameof(name));
			}

			Logger.LogInfo($"Found resource {resourceName}");

			using (Stream s = asm.GetManifestResourceStream(resourceName))
			{
				output = new byte[s.Length];
				s.Read(output, 0, output.Length);
				s.Dispose();
			}

			return output;
		}
    }
}
