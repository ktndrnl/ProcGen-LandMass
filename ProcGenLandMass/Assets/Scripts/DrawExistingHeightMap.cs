using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawExistingHeightMap : MonoBehaviour
{
	public Texture2D existingHeightMap;

	public GameObject chunkPrefab;
	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureData;
	public Material terrainMaterial;

	public bool ForceUpdate;

	private HeightMap[] heightMapChunks;
	private HashSet<GameObject> chunks = new HashSet<GameObject>();

	private void Start()
	{
		GenerateChunks();
	}

	private void OnValuesUpdated()
	{
		if (!Application.isPlaying)
		{
			GenerateChunks();
		}
	}

	private void OnTextureValuesUpdated()
	{
		textureData.ApplyToMaterial(terrainMaterial);
	}
	
	private void OnValidate()
	{
		if (meshSettings != null)
		{
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if (heightMapSettings != null)
		{
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if (textureData != null)
		{
			textureData.OnValuesUpdated -= OnTextureValuesUpdated;
			textureData.OnValuesUpdated += OnTextureValuesUpdated;
		}
		
	}

	private void GenerateChunks()
	{
		HeightMap[,] heightMaps = ImportHeightMap.ConvertToChunks(existingHeightMap, heightMapSettings, meshSettings);
		for (int y = 0, yCoord = 0; y < heightMaps.GetLength(1); y++, yCoord += (int)((meshSettings.numVerticesPerLine - 6) * meshSettings.meshScale))
		{
			for (int x = 0, xCoord = 0; x < heightMaps.GetLength(0); x++, xCoord += (int)((meshSettings.numVerticesPerLine - 6) * meshSettings.meshScale))
			{
				GameObject chunkObject = 
					Instantiate(chunkPrefab, new Vector3(yCoord, 0, xCoord), Quaternion.Euler(0, -90, 0), transform);
				chunkObject.GetComponent<MeshFilter>().sharedMesh =
					MeshGenerator.GenerateTerrainMesh(heightMaps[x, y].values, meshSettings, 0).CreateMesh();
			}
		}
	}
}
