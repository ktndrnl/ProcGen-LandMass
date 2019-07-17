using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameras : MonoBehaviour
{
    public GameObject previewCamera;
	public GameObject worldCamera;

	public void Start()
	{
		GameManager.instance.previewCamera = previewCamera;
		GameManager.instance.worldCamera = worldCamera;
	}
}
