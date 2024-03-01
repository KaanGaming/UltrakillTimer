using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UltrakillTimer
{
	public class PhaseChangeCollisionCheck : MonoBehaviour // For GameObjects with the name MoonExitArenaOrb
	{
		public static void AddComponentToPortals()
		{
			foreach (var obj in GameObject.FindObjectsOfType<GameObject>().Where(x => x.name.Contains("MoonExitArenaOrb")))
			{
				obj.AddComponent<PhaseChangeCollisionCheck>();
			}
		}

		public void OnTriggerEnter(Collider c)
		{
			UltrakillTimerPlugin.LogDebug($"Collision detected from {c.gameObject.name}");
			GameObject go = c.gameObject;
			if (go.GetComponent<CharacterBody>() != null)
				OnCollisionEnterEvent(c.gameObject);
		}

		public static event Action<GameObject> OnCollisionEnterEvent;
	}
}
