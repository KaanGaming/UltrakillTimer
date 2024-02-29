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
		public void OnCollisionEnter(Collision c)
		{
			GameObject go = c.gameObject;
			if (go.GetComponent<CharacterBody>() != null)
				OnCollisionEnterEvent(c.gameObject);
		}

		public static event Action<GameObject> OnCollisionEnterEvent;
	}
}
