
using System;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class ImportHeightMap
{
	private static float[,] heightMapArray;

	public static HeightMap[,] ConvertToChunks(Texture2D mapImage, HeightMapSettings heightMapSettings,
											  MeshSettings meshSettings)
	{
		// Chunks should share same end point (not included extra vertex used for normals) ex: c1v-c1v-c1v&c2n-c1v&c2v-c2v&c1n-c2v-c2v-...
		
		// numVertsPerLine includes 2 not rendered, but used for calculating normals. ex: xooox
		int numVertsPerLine = meshSettings.numVerticesPerLine;
		
		int numHorizontalChunksNeeded = 
			(mapImage.width + numVertsPerLine - 1) / numVertsPerLine;
		int numVerticalChunksNeeded = 
			(mapImage.height + numVertsPerLine - 1) / numVertsPerLine;
		int totalChunksNeeded = numHorizontalChunksNeeded * numVerticalChunksNeeded;
		
		HeightMap[,] heightMaps = new HeightMap[numHorizontalChunksNeeded,numVerticalChunksNeeded];
		float[,][,] heightMapValues = new float[numHorizontalChunksNeeded, numVerticalChunksNeeded][,];

		// Get maximum and minimum height values;
		Color[] mapImageColors = mapImage.GetPixels();
		float maxHeightValue = 
			ConvertColorsToHeightValues(
				mapImageColors, mapImage.width, mapImage.height, heightMapSettings).Cast<float>().Max();
		float minHeightValue = 
			ConvertColorsToHeightValues(
				mapImageColors, mapImage.width, mapImage.height, heightMapSettings).Cast<float>().Min();

		// Resize mapImage so it will be centered in the chunks it needs
		int newWidth = numHorizontalChunksNeeded * numVertsPerLine;
		int newHeight = numVerticalChunksNeeded * numVertsPerLine;
		Texture2D resizedMapImage = new Texture2D(newWidth, newHeight);
		Color[] blackFill = new Color[resizedMapImage.width * resizedMapImage.height];
		blackFill.Populate(Color.black);
		resizedMapImage.SetPixels(blackFill);

		int horizontalPadding = (newWidth - mapImage.width) / 2;
		int verticalPadding = (newHeight - mapImage.height) / 2;
		resizedMapImage.SetPixels(
			horizontalPadding, verticalPadding, mapImage.width, mapImage.height, mapImage.GetPixels());
		
		// Get pixel blocks corresponding to each chunk from resizedMapImage and convert them to 2d float arrays
		int uniqueVertsPerChunk = numVertsPerLine - 6;
		for (int yChunk = 0, x = 0, y = 0; yChunk < numVerticalChunksNeeded; yChunk++, y += uniqueVertsPerChunk, x = 0)
		{
			for (int xChunk = 0; xChunk < numHorizontalChunksNeeded; xChunk++, x += uniqueVertsPerChunk)
			{
				heightMapValues[xChunk, yChunk] = ConvertColorsToHeightValues(
					resizedMapImage.GetPixels(x, y, numVertsPerLine, numVertsPerLine),
					numVertsPerLine, numVertsPerLine, heightMapSettings);
			}
		}

		// Convert heightMapValues to HeightMaps
		for (int y = 0; y < numVerticalChunksNeeded; y++)
		{
			for (int x = 0; x < numHorizontalChunksNeeded; x++)
			{
				heightMaps[x, y] = 
					new HeightMap(heightMapValues[x, y], maxHeightValue, minHeightValue);
			}
		}

		return heightMaps;
	}

	private static float[,] ConvertColorsToHeightValues(Color[] colors, int width, int height, HeightMapSettings settings)
	{
		float[,] heightMapValues = new float[width,height];
		AnimationCurve heightCurve = new AnimationCurve(settings.heightCurve.keys);
		for (int y = 0, i = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++, i++)
			{
				float colorValue = colors[i].grayscale;
				colorValue *= heightCurve.Evaluate(colorValue) * settings.heightMultiplier;
				heightMapValues[x, y] = colorValue;
			}
		}

		return heightMapValues;
	}
	
	public static void Populate<T>(this T[] arr, T value ) {
		for ( int i = 0; i < arr.Length;i++ ) {
			arr[i] = value;
		}
	}

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
		int vertsPerLine = meshSettings.numVerticesPerLine;
		int numChunksX = Mathf.RoundToInt(importedHeightMap.values.GetLength(0) / vertsPerLine);
		int numChunksY = Mathf.RoundToInt(importedHeightMap.values.GetLength(1) / vertsPerLine);
		
		HeightMap[] heightMaps = new HeightMap[numChunksX * numChunksY];
		
		for (int x = numChunksX - 1, i = 0; x >= 0; x--)
		{
			for (int y = numChunksY - 1; y >= 0; y--, i++)
			{
				float[,] h = new float[vertsPerLine, vertsPerLine];
				int offsetX = x * vertsPerLine;
				int offsetY = y * vertsPerLine;
				for (int hx = 0; hx < vertsPerLine; hx++)
				{
					for (int hy = 0; hy < vertsPerLine; hy++)
					{
						try
						{
							if (x == numChunksX - 1 || y == numChunksY - 1)
							{
								h[hx, hy] = importedHeightMap.values[offsetX + hx, offsetY + hy];
							}
							else
							{
								h[hx, hy] = importedHeightMap.values[offsetX + hx - 1, offsetY + hy - 1];
							}
						}
						catch (IndexOutOfRangeException e)
						{
							h[hx, hy] = 1f;
						}
					}
				}

				heightMaps[i] = new HeightMap(h, importedHeightMap.minValue, importedHeightMap.maxValue);
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
