using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : Menu
{
	public event Action OnStartButton;

	public void OnStartButtonPressed()
	{
		OnStartButton?.Invoke();
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
