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
		}

		public static void CreateMusicController()
		{
			if (_gomc != null)
				return;

			_gomc = new GameObject("UltrakillTimer MusicController");
			_gomc.transform.SetParent(Camera.current.transform, false);
			GameObject.DontDestroyOnLoad(_gomc);
			_audiosrc = _gomc.AddComponent<AudioSource>();
			_audiosrc.minDistance = 1000;
			_audiosrc.maxDistance = 10000;

			SceneManager.activeSceneChanged += OnSceneChange;
		}

		private static GameObject _gomc;
		private static AudioSource _audiosrc;
		public static AudioClip phase1;
		public static AudioClip phase2;
		public static bool stopMusicOnSceneChange;

		public static float Time => _audiosrc.time;

		private static void OnSceneChange(Scene from, Scene to)
		{
			if (stopMusicOnSceneChange)
				_audiosrc.Stop();
		}

		public static void Play(AudioClip clip)
		{
			_audiosrc.clip = clip;
			_audiosrc.Play();
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
			if (phase == 1)
				Play(phase1);
			if (phase == 2)
				Play(phase2);
		}
	}
}
