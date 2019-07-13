
using System.Text;
using UnityEngine;

public static class ImportHeightMap
{
	private static float[,] heightMapArray;

	public static HeightMap GenerateHeightMap(Texture2D heightMapImage, MeshSettings meshSettings)
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

		for (int y = height - 1, iY = 0; y >= 0; y--, iY++)
		{
			for (int x = width - 1, iX = 0; x >= 0; x--)
			{

				if (x < horizontalPadding || y < verticalPadding)
				{
					heightMapArray[x, y] = 0f;
					continue;
				}
				if (x >= width - horizontalPadding || y >= height - verticalPadding)
				{
					heightMapArray[x, y] = 0f;
					continue;
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
				heightMapArray[x, y - verticalPadding / 2] = colorValue;
			}
		}
		
		HeightMap heightMap = new HeightMap(heightMapArray, 0, maxValue);
		return heightMap;
	}

	private static float ColorToGreyscaleFloat(Color color)
	{
		Vector3 floats = new Vector3(color.r, color.g, color.b);
		int numColors = 0;
		if (floats.x > 0)
		{
			numColors++;
		}

		if (floats.y > 0)
		{
			numColors++;
		}

		if (floats.z > 0)
		{
			numColors++;
		}
		float averagedColor = (floats.x + floats.y + floats.z) / numColors == 0 ? 1 : numColors;
		return averagedColor;
	}
}
