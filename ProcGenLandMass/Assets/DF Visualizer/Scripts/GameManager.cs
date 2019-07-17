﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
	#region Singleton

	public static GameManager instance;
	
	private void Awake()
	{
		if (instance != null)
		{
			Debug.LogWarning("Attempting to create more than one GameManager.");
			Destroy(this);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
	}

	#endregion

	public int worldSceneBuildIndex = 1;

	public event Action<UIState> OnGameStateChange; 

	[HideInInspector]
	public MainMenu mainMenu;

	[HideInInspector]
	public GameObject previewCamera;

	[HideInInspector]
	public GameObject worldCamera;
	
	private void Start()
	{
		SceneLoader.instance.LoadStartingScenes();
		SceneLoader.instance.OnLoadComplete += OnScenesLoaded;
		mainMenu = UIManager.instance.mainMenu;
		mainMenu.OnStartButton += StartGame;
	}

	// TODO: Replace. Just for testing.
	private bool isPaused;
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && UIManager.instance.uiState != UIState.MainMenu)
		{
			OnGameStateChange?.Invoke(isPaused ? UIState.World : UIState.PauseMenu);
			isPaused = !isPaused;
		}
	}

	private void OnScenesLoaded()
	{
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(worldSceneBuildIndex));
		OnGameStateChange?.Invoke(UIState.MainMenu);
	}

	private void StartGame()
	{
		OnGameStateChange?.Invoke(UIState.World);
		SwitchCameras();
	}
	
	private void SwitchCameras()
	{
		worldCamera.SetActive(!worldCamera.activeSelf);
		previewCamera.SetActive(!previewCamera.activeSelf);
	}

	public void ResumeGame()
	{
		OnGameStateChange?.Invoke(UIState.World);
		isPaused = false;
	}
	
	public void QuitToMainMenu()
	{
		OnGameStateChange?.Invoke(UIState.MainMenu);
		SwitchCameras();
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
