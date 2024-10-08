﻿using R2API.Utils;
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

		public static float GetVolume()
		{
			var volmastera = typeof(RoR2.AudioManager).GetField("cvVolumeMaster", BindingFlags.NonPublic | BindingFlags.Static);
			var volmusica = typeof(RoR2.AudioManager).GetField("cvVolumeMsx", BindingFlags.NonPublic | BindingFlags.Static);

			object volmasterconvar = volmastera.GetValue(null);
			float volmaster = float.Parse(volmasterconvar.InvokeMethod<string>("GetString"), CultureInfo.InvariantCulture) / 100f;
			object volmsxconvar = volmusica.GetValue(null);
			float volmusic = float.Parse(volmsxconvar.InvokeMethod<string>("GetString"), CultureInfo.InvariantCulture) / 100f;

			return (UltrakillTimerPlugin.IgnoreIGVolume ? 1 : volmusic) * volmaster;
		}

		private void Update()
		{
			object volmasterconvar = _volmaster.GetValue(null); // the classes are PRIVATE so we have to use object because we cant use the subclass
			float volmaster = float.Parse(volmasterconvar.InvokeMethod<string>("GetString"), CultureInfo.InvariantCulture) / 100f;
			object volmsxconvar = _volmusic.GetValue(null);
			float volmusic = float.Parse(volmsxconvar.InvokeMethod<string>("GetString"), CultureInfo.InvariantCulture) / 100f;

			_volume = volmaster * (UltrakillTimerPlugin.IgnoreIGVolume ? 1 : volmusic);
			_audiosrc.volume = volmaster * (UltrakillTimerPlugin.IgnoreIGVolume ? 1 : volmusic) * _volumemod * UltrakillTimerPlugin.MusicVolumeConfig;
		}

		private void OnDestroy()
		{
			On.RoR2.UI.PauseScreenController.OnDisable -= OnUnpause;
			On.RoR2.UI.PauseScreenController.OnEnable -= OnPause;
		}

		private void OnUnpause(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
		{
			UltrakillTimerPlugin.LogDebug("Unpaused! Adjusting volume mod to 1f");
			orig(self);
			StartCoroutine(ChangeVolumeGradually(1f, 0.5f));
		}

		private void OnPause(On.RoR2.UI.PauseScreenController.orig_OnEnable orig, RoR2.UI.PauseScreenController self)
		{
			UltrakillTimerPlugin.LogDebug("Paused! Adjusting volume mod to 0.25f");
			orig(self);
			StartCoroutine(ChangeVolumeGradually(0.25f, 0.5f));
		}

		private IEnumerator ChangeVolumeGradually(float volume, float t)
		{
			// TODO: this isnt working somehow and i need to fix it
			float ct = 0f;
			float oldvolumemod = _volumemod;
			while (ct < t)
			{
				UltrakillTimerPlugin.LogDebug($"{Time.unscaledDeltaTime} : {ct} / {t} .. {_volumemod} to {volume}");
				ct += Time.unscaledDeltaTime;
				_volumemod = Mathf.Lerp(oldvolumemod, volume, ct / t);
				_audiosrc.volume = _volume * _volumemod * UltrakillTimerPlugin.MusicVolumeConfig;
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
