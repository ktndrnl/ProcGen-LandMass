
using System;
using System.Text;
using UnityEngine;

public static class ImportHeightMap
{
	private static float[,] heightMapArray;

	public static ImportedHeightMap GenerateHeightMap(Texture2D heightMapImage, MeshSettings meshSettings)
	{
		int chunkSize = meshSettings.numVerticesPerLine;
		int numHorizontalChunksNeeded = 
			(heightMapImage.width + chunkSize - 1) / chunkSize;
		int numVerticalChunksNeeded = 
			(heightMapImage.height + chunkSize - 1) / chunkSize;

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
		
		HeightMap[] heightMaps = new HeightMap[numChunksX * numChunksY];
		float[,] heightMapValues = new float[meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine];
		var byteLength = sizeof(float) * heightMapValues.Length;

		for (int i = 0; i <  numChunksX * numChunksY; i++)
		{
			heightMapValues = new float[meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine];
			Buffer.BlockCopy(importedHeightMap.values, byteLength * i, heightMapValues, 0, byteLength);
			heightMaps[i] = new HeightMap(heightMapValues, 0, 1);
		}

		return heightMaps;

		/*for (int chunksY = 0, x = 0, y = 0; chunksY < numChunksY; chunksY++, y += meshSettings.numVerticesPerLine)
		{
			for (int chunksX = 0; chunksX < numChunksX; chunksX++, x += meshSettings.numVerticesPerLine)
			{
				for (int i = 0; i < meshSettings.numVerticesPerLine; i++, y++)
				{
					for (int j = 0; j < meshSettings.numVerticesPerLine; j++, x++)
					{
						heightMapValues[i, j] = importedHeightMap.values[x, y];
					}
				}
				heightMaps[numChunksY + numChunksX] = new HeightMap(heightMapValues, 0, 1);
			}
		}*/
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
