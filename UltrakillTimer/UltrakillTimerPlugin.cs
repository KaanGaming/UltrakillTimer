using BepInEx;
using RoR2;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

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

		public void Awake()
		{
			Logger.LogInfo("Loading asset bundle");
			_ab = AssetBundle.LoadFromMemory(GetEmbeddedResource("ultrakilltimerbundle"));
			Logger.LogInfo("Loading timer prefab...");
			_timer = _ab.LoadAsset<GameObject>("assets/critical failure timer.prefab");
			Logger.LogInfo($"Loaded prefab {_timer.name} successfully.");
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
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
				ctrl.timerEnd = 80;
			}
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
