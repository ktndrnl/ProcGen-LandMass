using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform followTarget;
	public float inertia;

	private void LateUpdate()
	{
		transform.position = Vector3.Lerp(transform.position, followTarget.position, inertia * Time.deltaTime);
		transform.rotation = Quaternion.Lerp(transform.rotation, followTarget.rotation, inertia * Time.deltaTime);
	}
}
