using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
	private void Start()
	{
		SceneLoader.instance.OnLoadComplete += Disable;
	}

	private void Disable()
	{
		gameObject.SetActive(false);
		SceneLoader.instance.OnLoadComplete -= Disable;
	}
}
