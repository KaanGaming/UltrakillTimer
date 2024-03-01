using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UltrakillTimer
{
	public static class MusicController
	{
		static MusicController()
		{
			stopMusicOnSceneChange = true;
			currentPhase = 0;

			SceneManager.activeSceneChanged += OnSceneChange;
		}

		public static void CreateMusicController()
		{
			if (_gomc != null)
				return;

			_gomc = new GameObject("UltrakillTimer MusicController");
			_audiosrc = _gomc.AddComponent<AudioSource>();
			_audiosrc.minDistance = 1000;
			_audiosrc.maxDistance = 10000;
			_audiosrc.loop = true;
			_gomc.AddComponent<AutoVolumeControl>();
			_camfollower = _gomc.AddComponent<AlwaysFollowCamera>();
		}

		private static GameObject _gomc;
		private static AudioSource _audiosrc;
		private static AlwaysFollowCamera _camfollower;
		public static AudioClip phase1;
		public static AudioClip phase2;
		public static bool stopMusicOnSceneChange;
		public static byte currentPhase { get; private set; }

		public static float Time
		{
			get
			{
				if (_audiosrc == null)
					return 0f;
				return _audiosrc.time;
			}
		}

		public static void AttemptLoadMusic()
		{
			void log(string msg) => UltrakillTimerPlugin.LogInfo(msg);
			void logerr(string msg) => UltrakillTimerPlugin.LogError(msg);

			log("Attempting to load audio data...");

			if (phase1 != null)
			{
				log("Loading phase 1 audio data");
				bool success = phase1.LoadAudioData();
				if (!success)
					logerr($"FAILURE!!!!!!!!! load state ended up to be {phase1.loadState}");
				else
					log($"Loaded phase 1 audio data successfully");
			}
			if (phase2 != null)
			{
				log("Loading phase 2 audio data");
				bool success = phase2.LoadAudioData();
				if (!success)
					logerr($"FAILURE!!!!!!!!! load state ended up to be {phase1.loadState}");
				else
					log($"Loaded phase 1 audio data successfully");
			}
		}

		private static void OnSceneChange(Scene from, Scene to)
		{
			if (to.name == "moon2")
				AttemptLoadMusic();

			if (_gomc == null)
				return;

			if (stopMusicOnSceneChange)
				Stop();
		}

		public static void Play(AudioClip clip)
		{
			if (_gomc == null || InstanceExists())
				CreateMusicController();

			_audiosrc.clip = clip;
			_audiosrc.Play();
		}

		public static void Stop()
		{
			if (_gomc == null || _audiosrc == null)
				return;

			_audiosrc.Stop();
			currentPhase = 0;
		}

		public static bool InstanceExists()
		{
			return _audiosrc != null;
		}

		public static void LoadPhase(byte phase, AudioClip clip)
		{
			if (phase == 1)
				phase1 = clip;
			if (phase == 2)
				phase2 = clip;
		}

		public static void PlayPhase(byte phase)
		{
			currentPhase = phase;

			if (phase == 1)
				Play(phase1);
			if (phase == 2)
				Play(phase2);
		}
	}
}
