using System;
using System.Collections;
using System.Collections.Generic;
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
	
	private static readonly int SlideIn = Animator.StringToHash("SlideIn");
	private static readonly int SlideOut = Animator.StringToHash("SlideOut");

	private void Start()
	{
		GameManager.instance.OnGameStateChange += OnGameStateChanged;
	}

	private void OnGameStateChanged(UIState state)
	{
		switch (state)
		{
			case UIState.MainMenu:
				EnableMenu(mainMenu, mainMenuAnimator);
				DisableMenu(pauseMenu, pauseMenuAnimator);
				break;
			case UIState.PauseMenu:
				EnableMenu(pauseMenu, pauseMenuAnimator);
				DisableMenu(mainMenu, mainMenuAnimator);
				break;
			case UIState.World:
				DisableMenu(mainMenu, mainMenuAnimator);
				DisableMenu(pauseMenu, pauseMenuAnimator);
				break;
		}
		
		uiState = state;
	}

	private void EnableMenu(Menu menu, Animator animator)
	{
		if (!menu.gameObject.activeSelf)
		{
			menu.gameObject.SetActive(true);
			animator.SetTrigger(SlideIn);
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
	}

	public void ToggleImportMenu()
	{
		importMenu.gameObject.SetActive(!importMenu.gameObject.activeSelf);
	}
}

public enum UIState
{
	MainMenu,
	PauseMenu,
	World
}
