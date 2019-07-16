using UnityEngine;
using UnityEngine.UI;

public class SceneLoadingBar : MonoBehaviour
{
	[SerializeField]
	private Slider loadingBarSlider;

	private void Start()
	{
		loadingBarSlider = GetComponent<Slider>();
		SceneLoader.instance.OnLoadProgressChanged += OnLoadProgressChanged;
	}

	private void OnLoadProgressChanged(float progress)
	{
		loadingBarSlider.value = progress;
	}
}
