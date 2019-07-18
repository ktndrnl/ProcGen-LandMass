﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
	public TerrainGenerator terrainGenerator;
	public Vector3 focalPoint = new Vector3(294f, 20f, 294f);
	public float focalPointDistanceMultiplier = 0.5f;
	public float cameraHeight = 180f;
	public float rotationSpeed = 5f;
	private bool rotationEnabled = true;
	private bool positionSet = false;

	private Vector3 mapCenter;

	private void Start()
	{
		//mapCenter = new Vector3(terrainGenerator.mapCenter, 0, terrainGenerator.mapCenter);
		//mapCenter = new Vector3(490, 0, 490);
		SetPosition();
	}

	private void Update()
	{
		if (rotationEnabled)
		{
			if (!positionSet)
			{
				SetPosition();
			}
			transform.LookAt(focalPoint);
			transform.RotateAround(focalPoint, Vector3.up, rotationSpeed * Time.deltaTime);
		}
	}

	public void EnableRotation()
	{
		if (rotationEnabled)
		{
			Debug.Log("Camera rotation already enabled.");
			return;
		}

		rotationEnabled = true;
		positionSet = false;
	}

	public void DisableRotation()
	{
		if (!rotationEnabled)
		{
			Debug.Log("Camera rotation already disabled.");
			return;
		}

		rotationEnabled = false;
		positionSet = false;
	}
	
	private void SetPosition()
	{
		transform.position = new Vector3(focalPoint.x * focalPointDistanceMultiplier, cameraHeight, 
			focalPoint.z * focalPointDistanceMultiplier);
		positionSet = true;
	}
}