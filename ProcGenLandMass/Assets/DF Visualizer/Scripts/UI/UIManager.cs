using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	
	#region Singleton

	public static UIManager instance;
	
	private void Awake()
	{
		if (instance != null)
		{
			Debug.LogWarning("Attempting to create more than one UIManager.");
			Destroy(this);
		}
		else
		{
			instance = this;
		}
	}

	#endregion

	// Main Menu
	[Header("Main Menu")]
	public MainMenu mainMenu;
	[SerializeField]
	private Animator mainMenuAnimator;
	
	// Pause Menu
	[Header("Pause Menu")]
	public PauseMenu pauseMenu;
	[SerializeField]
	private Animator pauseMenuAnimator;
	
	// Import Menu
	[Header("Import Menu")]
	public ImportMenu importMenu;

	[HideInInspector]
	public UIState uiState;

	public TextMeshProUGUI debugText;
	
	private static readonly int SlideIn = Animator.StringToHash("SlideIn");
	private static readonly int SlideOut = Animator.StringToHash("SlideOut");

	private void Start()
	{
		GameManager.instance.OnGameStateChange += OnGameStateChanged;
	}

	private void OnGameStateChanged(UIState state)
	{
		Debug.Log(uiState.ToString());
		switch (state)
		{
			case UIState.MainMenu:
				EnableMenu(mainMenu, mainMenuAnimator);
				DisableMenu(pauseMenu, pauseMenuAnimator);
				GameManager.instance.isPaused = false;
				break;
			case UIState.PauseMenu:
				EnableMenu(pauseMenu, pauseMenuAnimator);
				DisableMenu(mainMenu, mainMenuAnimator);
				GameManager.instance.isPaused = true;
				break;
			case UIState.World:
				DisableMenu(mainMenu, mainMenuAnimator);
				DisableMenu(pauseMenu, pauseMenuAnimator);
				DisableMenu(importMenu.gameObject);
				break;
		}
		
		uiState = state;
		Debug.Log(uiState.ToString());
		debugText.text = uiState.ToString();
	}

	private void EnableMenu(GameObject menuObject)
	{
		if (!menuObject.gameObject.activeSelf)
		{
			menuObject.gameObject.SetActive(true);
		}
	}
	
	private void EnableMenu(Menu menu, Animator animator)
	{
		if (!menu.gameObject.activeSelf)
		{
			menu.gameObject.SetActive(true);
			animator.SetTrigger(SlideIn);
		}
	}

	private void DisableMenu(GameObject menuObject)
	{
		if (menuObject.gameObject.activeSelf)
		{
			menuObject.gameObject.SetActive(false);
		}
	}
	
	private void DisableMenu(Menu menu, Animator animator)
	{
		if (menu.gameObject.activeSelf)
		{
			animator.SetTrigger(SlideOut);
			menu.SlideOutComplete += OnMenuSlideOutComplete;
		}
	}

	private void OnMenuSlideOutComplete(Menu menu)
	{
		menu.SlideOutComplete -= OnMenuSlideOutComplete;
		menu.gameObject.SetActive(false);
		GameManager.instance.isPaused = false;
	}

	public void ToggleMenu(GameObject menuObject)
	{
		menuObject.SetActive(!menuObject.activeSelf);
	}
}

public enum UIState
{
	MainMenu,
	PauseMenu,
	World
}
