
using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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

		HeightMap[,] heightMaps = new HeightMap[numHorizontalChunksNeeded, numVerticalChunksNeeded];
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
			0, 0, mapImage.width, mapImage.height, mapImage.GetPixels());

		// Get pixel blocks corresponding to each chunk from resizedMapImage and convert them to 2d float arrays
		int uniqueVertsPerChunk = numVertsPerLine - 3;
		
		/*NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

		NativeArray<int> arrayIndex = new NativeArray<int>(numVertsPerLine * numVertsPerLine, Allocator.TempJob);
		NativeArray<int> length = new NativeArray<int>(numVertsPerLine * numVertsPerLine, Allocator.TempJob);
		NativeArray<ColorArrayStruct> colorArrayStructs = new NativeArray<ColorArrayStruct>(numHorizontalChunksNeeded * numVerticalChunksNeeded, Allocator.Temp);
		
		for (int yChunk = 0, x = 0, y = 0; yChunk < numVerticalChunksNeeded; yChunk++, y += uniqueVertsPerChunk, x = 0)
		{
			for (int xChunk = 0, chunkNum = 0; xChunk < numHorizontalChunksNeeded; xChunk++, x += uniqueVertsPerChunk, chunkNum++)
			{
				colorArrayStructs[chunkNum] = new ColorArrayStruct
				{
					colorArray = resizedMapImage.GetPixels(x, y, numVertsPerLine, numVertsPerLine)
				};
				// Mirror image
				for (int i = 0, j = 0; i < numVertsPerLine * numVertsPerLine; i += numVertsPerLine, j++)
				{
					arrayIndex[j] = i;
					length[j] = numVertsPerLine;
				}
			}
		}*/

		// ReverseColorArrayJob reverseColorArrayJob = new ReverseColorArrayJob()
		// {
		// 	arrayIndex = arrayIndex,
		// 	colorArrayStructs = colorArrayStructs,
		// 	length = length
		// };
		// reverseColorArrayJob.Schedule()
		
		for (int yChunk = 0, x = 0, y = 0; yChunk < numVerticalChunksNeeded; yChunk++, y += uniqueVertsPerChunk, x = 0)
		{
			for (int xChunk = 0; xChunk < numHorizontalChunksNeeded; xChunk++, x += uniqueVertsPerChunk)
			{
				Color[] colors = resizedMapImage.GetPixels(x, y, numVertsPerLine, numVertsPerLine);
				// Mirror image
				for (int i = 0; i < colors.Length; i += numVertsPerLine)
				{
					Array.Reverse(colors, i, numVertsPerLine);
				}
				// Rotate image 180 degrees
				Array.Reverse(colors, 0, colors.Length);
				
				heightMapValues[xChunk, yChunk] = ConvertColorsToHeightValues(
					colors, numVertsPerLine, numVertsPerLine, heightMapSettings);
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
	
	/*public struct ColorArrayStruct
	{
		public Color[] colorArray;
	}

	[BurstCompile]
	private struct ReverseColorArrayJob : IJobParallelFor
	{
		public NativeArray<ColorArrayStruct> colorArrayStructs;
		public NativeArray<int> arrayIndex;
		public NativeArray<int> length;

		public void Execute(int index)
		{
			int i = arrayIndex[index];
			int j = arrayIndex[index] + length[index] - 1;
			Color[] colorArray = colorArrayStructs[index].colorArray;
			while (i < j)
			{
				Color temp = colorArray[i];
				colorArray[i] = colorArray[j];
				colorArray[j] = temp;
				i++;
				j--;
			}
		}
	}*/

	private static float[,] ConvertColorsToHeightValues(Color[] colors, int width, int height,
														HeightMapSettings settings)
	{
		float[,] heightMapValues = new float[width, height];
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

	public static void Populate<T>(this T[] arr, T value)
	{
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i] = value;
		}
	}
}
