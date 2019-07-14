using System;
using UnityEngine;

public class ImportedHeightMapViewer : MonoBehaviour
{
	public Texture2D heightMapImage;
	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public Renderer textureRenderer;
	public bool update;

	private void OnValidate()
	{
		textureRenderer.sharedMaterial.mainTexture = 
			TextureGenerator.TextureFromHeightMap(ImportHeightMap.GenerateHeightMap(heightMapImage, meshSettings, heightMapSettings));
	}
}
