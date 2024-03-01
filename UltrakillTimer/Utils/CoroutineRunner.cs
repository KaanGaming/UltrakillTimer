﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UltrakillTimer.Utils
{
	public class CoroutineRunner : MonoBehaviour
	{
		public static void RunCoroutine(IEnumerator coroutine, float destroytimer)
		{
			GameObject go = new GameObject("Coroutine Runner");
			var cr = go.AddComponent<CoroutineRunner>();
			cr.coroutine = coroutine;
			cr.timer = destroytimer;
			cr.StartCoroutineNow();
		}

		private void Awake()
		{
			StartCoroutine("DelayedDelayedDestroy");
		}

		public IEnumerator coroutine;
		public float timer;

		private void StartCoroutineNow()
		{
			StartCoroutine(coroutine);
		}

		private IEnumerator DelayedDelayedDestroy()
		{
			yield return new WaitForSeconds(0.25f);
			GameObject.Destroy(gameObject, timer - 0.25f);
		}
	}
}