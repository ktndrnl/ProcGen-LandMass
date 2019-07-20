using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using B83.Image.BMP;
using UnityEngine;

public static class ImageImporter
{
	private static readonly string dataPath = Application.dataPath;
	private static readonly string mapsFolderPath = Path.Combine(dataPath, "..", "Maps");
	
	public static List<Texture2D> GetImportedImages()
	{
		if (!Directory.Exists(mapsFolderPath))
		{
			Debug.Log("Creating Maps folder");
			Directory.CreateDirectory(mapsFolderPath);
		}
		else
		{
			Debug.Log("Maps folder already exists");
		}

		var imagePaths = Directory.EnumerateFiles(mapsFolderPath, "*-el.bmp");
		List<Texture2D> imageTextures = new List<Texture2D>();
			
		foreach (string imagePath in imagePaths)
		{
			Texture2D tex = LoadTexture(imagePath);
			tex.hideFlags = HideFlags.None;
			tex.filterMode = FilterMode.Point;
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.name = Path.GetFileName(imagePath);
			imageTextures.Add(tex);
		}

		return imageTextures;
	}
	
	private static Texture2D LoadTexture(string filePath)
	{
		Texture2D tex = null;

		if (File.Exists(filePath))
		{
			BMPLoader bmpLoader = new BMPLoader();
			//bmpLoader.ForceAlphaReadWhenPossible = true; //Uncomment to read alpha too

			//Load the BMP data
			BMPImage bmpImg = bmpLoader.LoadBMP(filePath);

			//Convert the Color32 array into a Texture2D
			tex = bmpImg.ToTexture2D();
		}
		return tex;
	}
}
