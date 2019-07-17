using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
	private Animator animator;

	private void Start()
	{
		SceneLoader.instance.OnLoadComplete += Disable;
		animator = GetComponent<Animator>();
	}

	private void Disable()
	{
		gameObject.SetActive(false);
		SceneLoader.instance.OnLoadComplete -= Disable;
	}
}
