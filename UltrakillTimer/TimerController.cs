using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UltrakillTimer
{
	public class TimerController : MonoBehaviour
	{
		private float _timer;
		public Run.FixedTimeStamp timerEnd;
		public Color originalColor;
		public Color flashColor;
		public float fadeTime;
		public float delay;
		//public AudioSource sync;
		private TextMeshProUGUI timerText;
		private TextMeshProUGUI headerText;

		public void Awake()
		{
			timerText = transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
			headerText = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
		}

		public void Update()
		{
			_timer += Time.deltaTime;

			float _ftimer = _timer;
			if (MusicController.InstanceExists())
				_ftimer = MusicController.Time;

			float remainingTime = timerEnd.timeUntilClamped;
			string[] timerstr = remainingTime.ToString(CultureInfo.InvariantCulture).Split('.');
			timerText.text = $"{timerstr[0]}<size=40%>.{timerstr[1].Substring(0, timerstr[1].Length > 2 ? 2 : timerstr[1].Length)}";
			timerText.color = Color.Lerp(flashColor, originalColor, _ftimer % fadeTime + delay);
			headerText.color = Color.Lerp(flashColor, originalColor, _ftimer % fadeTime + delay);
		}

		public void ResetTimer()
		{
			_timer = 0f;
		}
	}
}
