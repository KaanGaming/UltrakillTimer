using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UltrakillTimer
{
	public class AutoVolumeControl : MonoBehaviour
	{
		private void Awake()
		{
			_volumemod = 1f;
			_audiosrc = GetComponent<AudioSource>();
			_volmaster = typeof(RoR2.AudioManager).GetField("cvVolumeMaster", BindingFlags.NonPublic | BindingFlags.Static);
			_volmusic = typeof(RoR2.AudioManager).GetField("cvVolumeMsx", BindingFlags.NonPublic | BindingFlags.Static);
			On.RoR2.UI.PauseScreenController.OnDisable += OnUnpause;
			On.RoR2.UI.PauseScreenController.OnEnable += OnPause;
		}

		private AudioSource _audiosrc;
		private FieldInfo _volmaster;
		private FieldInfo _volmusic;
		private float _volume;
		private float _volumemod;

		private void FixedUpdate()
		{
			// the game sets timestep to 0.00 when the game is paused so we use FixedUpdate to keep updating volume
			object volmasterconvar = _volmaster.GetValue(null); // the classes are PRIVATE so we have to use object because we cant use the subclass
			float volmaster = float.Parse(volmasterconvar.InvokeMethod<string>("GetString"), CultureInfo.InvariantCulture) / 100f;
			object volmsxconvar = _volmusic.GetValue(null);
			float volmusic = float.Parse(volmsxconvar.InvokeMethod<string>("GetString"), CultureInfo.InvariantCulture) / 100f;

			//_volume = volmaster * volmusic;
			_audiosrc.volume = volmaster * volmusic;
		}

		private void OnDestroy()
		{
			On.RoR2.UI.PauseScreenController.OnDisable -= OnUnpause;
			On.RoR2.UI.PauseScreenController.OnEnable -= OnPause;
		}

		private void OnUnpause(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
		{
			orig(self);
			StartCoroutine(ChangeVolumeGradually(1f, 0.5f));
		}

		private void OnPause(On.RoR2.UI.PauseScreenController.orig_OnEnable orig, RoR2.UI.PauseScreenController self)
		{
			orig(self);
			StartCoroutine(ChangeVolumeGradually(0.25f, 0.5f));
		}

		private IEnumerator ChangeVolumeGradually(float volume, float t)
		{
			float ct = 0f;
			float oldvolumemod = _volumemod;
			while (ct < t)
			{
				ct += Time.unscaledDeltaTime;
				_volumemod = Mathf.Lerp(oldvolumemod, volume, ct / t);
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
