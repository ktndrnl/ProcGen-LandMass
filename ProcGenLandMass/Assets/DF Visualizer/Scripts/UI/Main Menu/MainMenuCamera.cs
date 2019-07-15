using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
	public TerrainGenerator terrainGenerator;

	private Vector3 mapCenter;

	private void Start()
	{
		mapCenter = new Vector3(terrainGenerator.mapCenter, 0, terrainGenerator.mapCenter);
		transform.position = new Vector3(mapCenter.x * 0.5f, 90, mapCenter.z * 0.5f);
	}

	private void Update()
	{
		transform.LookAt(mapCenter);
		transform.RotateAround(mapCenter, Vector3.up, 5 * Time.deltaTime);
	}
}
