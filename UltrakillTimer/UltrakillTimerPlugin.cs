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
		private GameObject _timer;
		private GameObject _currenttimer;
		private Run.FixedTimeStamp _endtime;

		public void Awake()
		{
			Logger.LogInfo("Loading asset bundle");
			_ab = AssetBundle.LoadFromMemory(GetEmbeddedResource("ultrakilltimerbundle"));
			Logger.LogInfo("Loading timer prefab...");
			_timer = _ab.LoadAsset<GameObject>("assets/critical failure timer.prefab");
			Logger.LogInfo($"Loaded prefab {_timer.name} successfully.");

			On.RoR2.EscapeSequenceController.SetHudCountdownEnabled += SetHUDCountdownEnabled;
			On.RoR2.EscapeSequenceController.EscapeSequenceMainState.OnEnter += EscapeSeqMainStateOnEnter;
			PhaseChangeCollisionCheck.OnCollisionEnterEvent += OnEnterArenaPortal;
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				CreateTimer(Run.FixedTimeStamp.now + 80f);
			}
		}

		private void OnEnterArenaPortal(GameObject obj)
		{
			if (MusicController.InstanceExists())
			{
				MusicController.PlayPhase(2);
			}
		}

		private void EscapeSeqMainStateOnEnter(On.RoR2.EscapeSequenceController.EscapeSequenceMainState.orig_OnEnter orig, EscapeSequenceController.EscapeSequenceMainState self)
		{
			orig(self);
			_endtime = self.GetFieldValue<Run.FixedTimeStamp>("endTime");
		}

		private void SetHUDCountdownEnabled(On.RoR2.EscapeSequenceController.orig_SetHudCountdownEnabled orig, EscapeSequenceController self, RoR2.UI.HUD hud, bool shouldEnableCountdownPanel)
		{
			if (_currenttimer != shouldEnableCountdownPanel)
			{
				if (shouldEnableCountdownPanel)
				{
					CreateTimer(_endtime);
				}
				else
				{
					GameObject.Destroy(_currenttimer);
				}
			}
		}

		private void CreateTimer(Run.FixedTimeStamp endTime)
		{
			GameObject canvasobj = new GameObject("Timer Canvas");
			var canvas = canvasobj.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.referencePixelsPerUnit = 100;
			canvas.scaleFactor = 1;

			var timer = GameObject.Instantiate(_timer, canvas.transform);
			var ctrl = timer.AddComponent<TimerController>();
			ctrl.delay = 0;
			ctrl.fadeTime = 1;
			ctrl.flashColor = new Color(1, 1, 0, 1);
			ctrl.originalColor = new Color(1, 0, 0, 1);
			ctrl.timerEnd = endTime;
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
