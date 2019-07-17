using System;
using UnityEngine;

public class Menu : MonoBehaviour
{
	public event Action SlideInComplete;
	public event Action<Menu> SlideOutComplete;

	private void OnSlideInComplete()
	{
		SlideInComplete?.Invoke();
	}

	private void OnSlideOutComplete()
	{
		SlideOutComplete?.Invoke(this);
	}
}
