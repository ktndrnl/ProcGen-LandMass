using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	#region Singleton

	public static SceneLoader instance;

	private void Awake()
	{
		if (instance != null)
		{
			Debug.LogWarning("Trying to create more than one SceneLoader.");
			Destroy(this);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this);
	}
	#endregion
	
	[SerializeField]
	private int[] scenesToLoadAtStart;

	public event Action<float> OnLoadProgressChanged;
	public event Action OnLoadComplete;

	private float sceneLoadProgress;
	private float maxLoadingProgress;

	private Dictionary<int, bool> isSceneLoaded = new Dictionary<int, bool>();

	public void LoadStartingScenes()
	{
		StartCoroutine(LoadScenesAsync(scenesToLoadAtStart));
	}
	
	public IEnumerator LoadScenesAsync(int[] sceneBuildIndexes)
	{
		maxLoadingProgress = sceneBuildIndexes.Length * 0.9f;

		foreach (int sceneBuildIndex in sceneBuildIndexes)
		{
			isSceneLoaded.Add(sceneBuildIndex, false);
			StartCoroutine(LoadSceneAsync(sceneBuildIndex));
		}

		while (isSceneLoaded.ContainsValue(false))
		{
			OnLoadProgressChanged?.Invoke(sceneLoadProgress);
			yield return null;
		}
		
		isSceneLoaded.Clear();
		sceneLoadProgress = 0f;
		maxLoadingProgress = 0f;
		
		OnLoadComplete?.Invoke();
	}

	private IEnumerator LoadSceneAsync(int sceneBuildIndex)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
		while (!operation.isDone)
		{
			sceneLoadProgress += Mathf.Clamp01(operation.progress / maxLoadingProgress);
			yield return null;
		}

		isSceneLoaded[sceneBuildIndex] = true;
	}
}
