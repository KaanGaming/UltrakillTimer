using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UltrakillTimer
{
	public class AlwaysFollowCamera : MonoBehaviour
	{
		public void Awake()
		{
			StartFollowingCamera();
		}

		public bool keepFollowing = true;

		private IEnumerator FollowCamera()
		{
			if (Camera.main != null)
				transform.SetParent(Camera.main.transform, false);

			yield return new WaitForSeconds(3f);

			if (keepFollowing)
				StartCoroutine("FollowCamera");
		}

		private void StartFollowingCamera()
		{
			StartCoroutine("FollowCamera");
		}
	}
}
