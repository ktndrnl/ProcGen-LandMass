using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public Animator mainMenuAnimator;
	public float slideInWaitTime = 3f;

	private void Start()
	{
		StartCoroutine(SlideIn());
	}

	private IEnumerator SlideIn()
	{
		yield return new WaitForSeconds(slideInWaitTime);
		mainMenuAnimator.SetTrigger("SlideIn");
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
