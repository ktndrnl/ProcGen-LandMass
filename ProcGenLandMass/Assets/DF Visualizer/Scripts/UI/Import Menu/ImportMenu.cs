using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImportMenu : MonoBehaviour
{
	public GameObject fileButtonPrefab;
	public RectTransform fileListPanel;

	public GameObject filePreviewPanel;
	public RawImage filePreviewPanelImage;

	public void CreateFileButton(Texture2D fileTexture)
	{
		GameObject fileButton = Instantiate(fileButtonPrefab, fileListPanel);
		fileButton.GetComponent<FileButton>().PreviewImageTexture = fileTexture;
	}

	public void ShowFilePreview(RawImage fileImage)
	{
		filePreviewPanel.SetActive(true);
		filePreviewPanelImage.texture = fileImage.texture;
	}

	public void LoadImageIntoMap()
	{
		GameManager.instance.SendImageToMapGenerator((Texture2D)filePreviewPanelImage.texture);
	}
}
