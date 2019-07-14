
using System;
using System.Text;
using UnityEngine;

public static class ImportHeightMap
{
	private static float[,] heightMapArray;

	public static ImportedHeightMap GenerateHeightMap(Texture2D heightMapImage, MeshSettings meshSettings, HeightMapSettings heightMapSettings)
	{
		int chunkSize = meshSettings.numVerticesPerLine;
		int numHorizontalChunksNeeded = 
			(heightMapImage.width + chunkSize - 1) / chunkSize;
		int numVerticalChunksNeeded = 
			(heightMapImage.height + chunkSize - 1) / chunkSize;
		
		AnimationCurve heightCurve_threadsafe = new AnimationCurve(heightMapSettings.heightCurve.keys);

		int width = numHorizontalChunksNeeded * chunkSize;
		int height = numVerticalChunksNeeded * chunkSize;
		
		heightMapArray = new float[width, height];

		int horizontalPadding = (width - heightMapImage.width) / 2;
		int verticalPadding = (height - heightMapImage.height) / 2;

		float maxValue = float.MinValue;
		float minValue = float.MaxValue;

		for (int y = height - 1, iY = 0; y >= 0; y--, iY++)
		{
			for (int x = width - 1, iX = 0; x >= 0; x--)
			{

				if (x <= horizontalPadding || y <= verticalPadding)
				{
					
					heightMapArray[x, y] = 0f;
					continue;
				}
				if (x > heightMapImage.width + horizontalPadding || y > heightMapImage.height + verticalPadding)
				{
					heightMapArray[x, y] = 0f;
				}

				Color pixelColor = heightMapImage.GetPixel(iX++, iY);
				float colorValue;
				if (pixelColor.r == 0 && pixelColor.g == 0)
				{
					colorValue = pixelColor.b * 0.3f;
				}
				else
				{
					colorValue = pixelColor.grayscale;
				}

				colorValue *= heightCurve_threadsafe.Evaluate(colorValue) * heightMapSettings.heightMultiplier;

				if (colorValue > maxValue)
				{
					maxValue = colorValue;
				}

				if (colorValue < minValue)
				{
					minValue = colorValue;
				}
				heightMapArray[x - horizontalPadding, y - verticalPadding] = colorValue;
			}
		}

		ImportedHeightMap heightMap = new ImportedHeightMap(heightMapArray, minValue, maxValue);
		return heightMap;
	}

	public static HeightMap[] ChunkImportedHeightMap(ImportedHeightMap importedHeightMap, MeshSettings meshSettings)
	{
		int numChunksX = Mathf.RoundToInt(importedHeightMap.values.GetLength(0) / meshSettings.numVerticesPerLine);
		int numChunksY = Mathf.RoundToInt(importedHeightMap.values.GetLength(1) / meshSettings.numVerticesPerLine);
		int vertsPerLine = meshSettings.numVerticesPerLine;
		
		HeightMap[] heightMaps = new HeightMap[numChunksX * numChunksY];
		
		for (int x = 0, i = 0; x < numChunksX; x++)
		{
			for (int y = 0; y < numChunksY; y++, i++)
			{
				float[,] h = new float[vertsPerLine, vertsPerLine];
				int offsetX = x * vertsPerLine;
				int offsetY = y * vertsPerLine;
				for (int hx = 0; hx < vertsPerLine; hx++)
				{
					for (int hy = 0; hy < vertsPerLine; hy++)
					{
						h[hx, hy] = importedHeightMap.values[offsetX + hx, offsetY + hy];
					}
				}

				float maxValue = float.MinValue;
				float minValue = float.MaxValue;

				for (int j = 0; j < h.GetLength(0); j++)
				{
					for (int k = 0; k < h.GetLength(1); k++)
					{
						if (h[j,k] > maxValue)
						{
							maxValue = h[j, k];
						}

						if (h[j,k] < minValue)
						{
							minValue = h[j, k];
						}
					}
				}
				
				heightMaps[i] = new HeightMap(h, minValue, maxValue);
			}
		}

		return heightMaps;
	}
}

public struct ImportedHeightMap
{
	public readonly float[,] values;
	public readonly float minValue;
	public readonly float maxValue;

	public ImportedHeightMap(float[,] values, float minValue, float maxValue)
	{
		this.values = values;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}
}
