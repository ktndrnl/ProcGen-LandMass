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
		if (transform.childCount > 0)
		{
			foreach (Transform child in transform.GetComponentsInChildren<Transform>())
			{
				if (child.gameObject != this.gameObject)
				{
					DestroyImmediate(child.gameObject);
				}
			}
		}
		HeightMap[,] heightMaps = ImportHeightMap.ConvertToChunks(existingHeightMap, heightMapSettings, meshSettings).heightMaps;
		PlaceChunks(heightMaps);
	}

	private void PlaceChunks(HeightMap[,] heightMaps)
	{
		float posIncrement = (meshSettings.numVerticesPerLine - 3) * meshSettings.meshScale;
		// Chunks are placed on XZ plane (X = Unity's X, Y = Unity's Z)
		float zCoord = posIncrement * 0.5f + 1.5f;
		for (int y = 0; y < heightMaps.GetLength(1); y++, zCoord += posIncrement)
		{
			float xCoord = posIncrement * 0.5f + 1.5f;
			for (int x = 0; x < heightMaps.GetLength(0); x++, xCoord += posIncrement)
			{
				GameObject chunkObject =
					Instantiate(chunkPrefab, new Vector3(xCoord, 0, zCoord), Quaternion.identity, transform);
				chunkObject.GetComponent<MeshFilter>().sharedMesh =
					MeshGenerator.GenerateTerrainMesh(heightMaps[x, y].values, meshSettings, 0).CreateMesh();
			}
		}
	}
}
