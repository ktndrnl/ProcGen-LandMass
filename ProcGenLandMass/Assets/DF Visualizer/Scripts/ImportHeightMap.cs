
using System;
using System.IO;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class ImportHeightMap
{
	private static float[,] heightMapArray;

	public static HeightMapsData ConvertToChunks(Texture2D mapImage, HeightMapSettings heightMapSettings,
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
		float maxHeightValue = float.MinValue;
		float minHeightValue = float.MaxValue;

		/*int numJobs = mapImage.height;
		NativeArray<Color32> rawMapImage = mapImage.GetRawTextureData<Color32>();
		NativeArray<Color32> rawImageDataOutput = new NativeArray<Color32>(rawMapImage.Length, Allocator.TempJob);
		NativeArray<int> indexArray = new NativeArray<int>(numJobs, Allocator.TempJob);
		NativeArray<int> lengthArray = new NativeArray<int>(numJobs, Allocator.TempJob);

		for (int jobIndex = 0, arrayIndex = 0; jobIndex < numJobs; jobIndex++, arrayIndex += mapImage.width)
		{
			indexArray[jobIndex] = arrayIndex;
			lengthArray[jobIndex] = mapImage.width;
		}
		
		FlipImageJob flipImageJob = new FlipImageJob
		{
			rawImageDataInput = rawMapImage,
			rawImageDataOutput = rawImageDataOutput,
			indexArray = indexArray,
			lengthArray = lengthArray
		};

		JobHandle flipImageJobHandle;

		try
		{
			flipImageJobHandle = flipImageJob.Schedule(mapImage.height, mapImage.width);
			flipImageJobHandle.Complete();
		}
		catch (Exception e)
		{
			indexArray.Dispose();
			lengthArray.Dispose();
			rawImageDataOutput.Dispose();
			Console.WriteLine(e);
			throw;
		}
		
		Color32[] jobResult = flipImageJob.rawImageDataOutput.ToArray();
		Color[] jobResultColors = new Color[mapImage.height * mapImage.width];

		for (int i = 0; i < jobResultColors.Length; i++)
		{
			jobResultColors[i] = jobResult[i];
		}
		
		Texture2D tex = new Texture2D(mapImage.width, mapImage.height);
		tex.SetPixels(jobResultColors);
		tex.Apply();
		
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/../SavedHeightMap.png", bytes);
		
		indexArray.Dispose();
		lengthArray.Dispose();
		rawImageDataOutput.Dispose();*/

		// Resize mapImage so it will be centered in the chunks it needs
		int newWidth = numHorizontalChunksNeeded * numVertsPerLine;
		int newHeight = numVerticalChunksNeeded * numVertsPerLine;
		Texture2D resizedMapImage = new Texture2D(newWidth, newHeight);
		Color[] blackFill = new Color[resizedMapImage.width * resizedMapImage.height];
		blackFill.Populate(Color.black);
		resizedMapImage.SetPixels(blackFill);

		// int horizontalPadding = (newWidth - mapImage.width) / 2;
		// int verticalPadding = (newHeight - mapImage.height) / 2;
		
		resizedMapImage.SetPixels(0, 0, mapImage.width, mapImage.height, mapImage.GetPixels());

		// resizedMapImage.SetPixels(
		// 	0, 0, mapImage.width, mapImage.height, mapImage.GetPixels());

		// Get pixel blocks corresponding to each chunk from resizedMapImage and convert them to 2d float arrays
		int uniqueVertsPerChunk = numVertsPerLine - 3;

		Vector2 highestPointChunkCoord = new Vector2();

		for (int yChunk = 0, x = 0, y = 0; yChunk < numVerticalChunksNeeded; yChunk++, y += uniqueVertsPerChunk, x = 0)
		{
			for (int xChunk = 0; xChunk < numHorizontalChunksNeeded; xChunk++, x += uniqueVertsPerChunk)
			{
				Color[] colors = resizedMapImage.GetPixels(x, y, numVertsPerLine, numVertsPerLine);
				heightMapValues[xChunk, yChunk] = ConvertColorsToHeightValues(
					colors, numVertsPerLine, numVertsPerLine, heightMapSettings, ref maxHeightValue,
					ref minHeightValue, new Vector2(xChunk, yChunk), ref highestPointChunkCoord);
			}
		}

		// Normal one
		/*for (int yChunk = 0, x = 0, y = 0; yChunk < numVerticalChunksNeeded; yChunk++, y += uniqueVertsPerChunk, x = 0)
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
					colors, numVertsPerLine, numVertsPerLine, heightMapSettings, ref maxHeightValue, ref minHeightValue);
			}
		}*/

		// Convert heightMapValues to HeightMaps
		for (int y = 0; y < numVerticalChunksNeeded; y++)
		{
			for (int x = 0; x < numHorizontalChunksNeeded; x++)
			{
				heightMaps[x, y] =
					new HeightMap(heightMapValues[x, y], maxHeightValue, minHeightValue);
			}
		}

		HeightMapsData heightMapsData = new HeightMapsData
		{
			heightMaps = heightMaps,
			highestPointChunkCoord = highestPointChunkCoord
		};
		return heightMapsData;
	}
	
	public struct HeightMapsData
	{
		public HeightMap[,] heightMaps;
		public Vector2 highestPointChunkCoord;
	}
	
	[BurstCompile]
	private struct FlipImageJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		[ReadOnly]
		public NativeArray<Color32> rawImageDataInput;
		[NativeDisableParallelForRestriction]
		[WriteOnly]
		public NativeArray<Color32> rawImageDataOutput;
		public NativeArray<int> indexArray;
		public NativeArray<int> lengthArray;
		
		public void Execute(int index)
        {
        	int i = indexArray[index];
        	int j = indexArray[index] + lengthArray[index] - 1;
        	while (i < j)
        	{
        		Color32 temp = rawImageDataInput[i];
				rawImageDataOutput[i] = rawImageDataInput[j];
        		rawImageDataOutput[j] = temp;
        		i++;
        		j--;
        	}
        }
	}

	private static float[,] ConvertColorsToHeightValues(Color[] colors, int width, int height,
														HeightMapSettings settings, ref float maxHeightValue, 
														ref float minHeightValue, Vector2 chunkCoord, ref Vector2 highestPointChunkCoord)
	{
		float[,] heightMapValues = new float[width, height];
		AnimationCurve heightCurve = new AnimationCurve(settings.heightCurve.keys);
		for (int y = 0, i = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++, i++)
			{
				float colorValue = colors[i].grayscale;
				colorValue *= heightCurve.Evaluate(colorValue) * settings.heightMultiplier;
				
				if (colorValue > maxHeightValue)
				{
					maxHeightValue = colorValue;
					highestPointChunkCoord = chunkCoord;
				}
				if (colorValue < minHeightValue)
				{
					minHeightValue = colorValue;
				}
				
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
