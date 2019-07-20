using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileButton : MonoBehaviour
{
	[SerializeField]
	private RawImage previewImage;
	[SerializeField]
	private TextMeshProUGUI filenameText;

	[SerializeField]
	private ImportMenu importMenu;

	private Texture2D previewImageTexture;
	public Texture2D PreviewImageTexture
	{
		get => previewImageTexture;
		set
		{
			previewImageTexture = value;
			previewImage.texture = previewImageTexture;
			filenameText.text = previewImageTexture.name;
		}
	}

	private void Start()
	{
		importMenu = FindObjectOfType<ImportMenu>();
	}

	public void ShowFilePreview()
	{
		importMenu.ShowFilePreview(previewImage);
	}
}
