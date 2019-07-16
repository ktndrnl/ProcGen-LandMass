using System;
using System.Collections;
using System.Collections.Generic;
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

	public int worldSceneBuildIndex = 2;
	
	private void Start()
	{
		SceneLoader.instance.LoadStartingScenes();
		SceneLoader.instance.OnLoadComplete += OnScenesLoaded;
	}

	private void OnScenesLoaded()
	{
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(worldSceneBuildIndex));
	}
}
