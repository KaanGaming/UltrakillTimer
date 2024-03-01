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
using MonoMod.RuntimeDetour;
using UnityEngine.SceneManagement;
using BepInEx.Logging;

namespace UltrakillTimer
{
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

		public void Awake()
		{
			_log = Logger;

			Logger.LogInfo("Loading timer asset bundle");
			_ab = AssetBundle.LoadFromMemory(GetEmbeddedResource("ultrakilltimerbundle"));
			Logger.LogInfo("Loading timer prefab...");
			_timer = _ab.LoadAsset<GameObject>("assets/critical failure timer.prefab");
			Logger.LogInfo($"Loaded prefab {_timer.name} successfully.");

			// TODO: Set up options for the user to disable loading this
			Logger.LogInfo("Loading escape music asset bundle");
			_abm = AssetBundle.LoadFromMemory(GetEmbeddedResource("ultrakilltimersoundbundle"));
			Logger.LogInfo("Loading music...");
			MusicController.phase1 = _abm.LoadAsset<AudioClip>("assets/centaur b-4.ogg");
			Logger.LogInfo("Phase 1 music loaded");
			MusicController.phase2 = _abm.LoadAsset<AudioClip>("assets/centaur b-5.ogg");
			Logger.LogInfo("Phase 2 music loaded");
			Logger.LogInfo("Loaded escape music successfully.");

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
		#endregion

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				CreateTimer(Run.FixedTimeStamp.now + 80f);
			}
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
					GameObject.Destroy(_currenttimer);
				}
			}
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
			var ctrl = timer.AddComponent<TimerController>();
			ctrl.delay = 0;
			ctrl.fadeTime = 1;
			ctrl.flashColor = new Color(1, 1, 0, 1);
			ctrl.originalColor = new Color(1, 0, 0, 1);
			ctrl.timerEnd = endTime;

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
