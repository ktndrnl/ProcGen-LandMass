using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxSun : MonoBehaviour
{
	public Light directionalLight;
	public Transform player;
	public float distance;
	
	private Vector3 playerOffset;
	private Quaternion lightRot;
	private Vector3 dirToLight;

	private void Start()
	{
		lightRot = directionalLight.transform.rotation;
		transform.rotation = Quaternion.Euler(lightRot.eulerAngles.x - 180, 0, 0);
		dirToLight = transform.forward;
		playerOffset = dirToLight * distance;
		transform.position = player.position + playerOffset;
	}

	private void Update()
	{
		transform.position = player.position + playerOffset;
	}
}
