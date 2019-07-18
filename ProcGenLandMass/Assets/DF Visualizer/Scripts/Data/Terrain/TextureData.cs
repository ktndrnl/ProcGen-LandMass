﻿using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class TextureData : UpdateableData
{
	private static readonly int MinHeight = Shader.PropertyToID("minHeight");
	private static readonly int MaxHeight = Shader.PropertyToID("maxHeight");

	private const int textureSize = 512;
	private const TextureFormat textureFormat = UnityEngine.TextureFormat.RGB565;
	
	public Layer[] layers;

	private float savedMinHeight;
	private float savedMaxHeight;

	public void ApplyToMaterial(Material material)
	{
		material.SetInt("layerCount", layers.Length);
		material.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
		material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
		material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
		material.SetFloatArray("baseColorStrengths", layers.Select(x => x.tintStrength).ToArray());
		material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
		Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.Texture).ToArray());
		material.SetTexture("baseTextures", texturesArray);

		UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
	}

	public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
	{
		savedMinHeight = minHeight;
		savedMaxHeight = maxHeight;
		
		material.SetFloat("minHeight", minHeight);
		material.SetFloat("maxHeight", maxHeight);
	}

	private Texture2DArray GenerateTextureArray(Texture2D[] textures)
	{
		Texture2DArray textureArray = 
			new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
		for (int i = 0; i < textures.Length; i++)
		{
			textureArray.SetPixels(textures[i].GetPixels(), i);
		}
		textureArray.Apply();
		return textureArray;
	}
	
	[Serializable]
	public class Layer
	{
		public bool useTexture;
		[SerializeField]
		private Texture2D texture;
		[HideInInspector]
		public Texture2D Texture
		{
			get
			{
				if (useTexture)
				{
					return texture;
				}
				else
				{
					texture = new Texture2D(textureSize, textureSize);
					return texture;
				}
			}
		}

		public Color tint;
		[Range(0, 1)]
		public float tintStrength;
		[Range(0, 1)]
		public float startHeight;
		[Range(0, 1)]
		public float blendStrength;
		public float textureScale;
	}
}