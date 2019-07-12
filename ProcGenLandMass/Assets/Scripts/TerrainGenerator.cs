﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
	
	private const float viewerMoveThresholdForChunkUpdate = 25f;
	private const float sqrViewerMoveThresholdForChunkUpdate =
		viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;

	public Transform viewer;
	public Material mapMaterial;

	private Vector3 viewerPosition;
	private Vector3 viewerPositionOld;
	private float meshWorldSize;
	private int chunksVisibleInViewDst;

	private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	private HashSet<TerrainChunk> visibleTerrainChunks = new HashSet<TerrainChunk>();
	private HashSet<TerrainChunk> visibleTerrainChunksToRemove = new HashSet<TerrainChunk>();
	private bool iteratingOverVisibleTerrainChunks;

	private void Start()
	{
		textureSettings.ApplyToMaterial(mapMaterial);
		textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
		
		float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
		
		UpdateVisibleChunks();
	}

	private void Update()
	{
		viewerPosition = new Vector3(viewer.position.x, viewer.position.z, 0);

		if (viewerPosition != viewerPositionOld)
		{
			foreach(TerrainChunk chunk in visibleTerrainChunks)
			{
				chunk.UpdateCollisionMesh();
			}
		}
		
		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
		{
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	private void UpdateVisibleChunks()
	{
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
		iteratingOverVisibleTerrainChunks = true;
		foreach (TerrainChunk chunk in visibleTerrainChunks)
		{
			alreadyUpdatedChunkCoords.Add(chunk.coord);
			chunk.UpdateTerrainChunk();
		}
		iteratingOverVisibleTerrainChunks = false;

		foreach (TerrainChunk chunk in visibleTerrainChunksToRemove)
		{
			visibleTerrainChunks.Remove(chunk);
		}
		
		visibleTerrainChunksToRemove.Clear();
		
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
				{
					if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
					{
						terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					}
					else
					{
						TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, 
								detailLevels, colliderLODIndex, transform, viewer, mapMaterial);
						terrainChunkDictionary.Add(viewedChunkCoord,newChunk);
						newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
						newChunk.Load();
					}
				}
			}
		}
	}

	private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
	{
		if (isVisible)
		{
			visibleTerrainChunks.Add(chunk);
		}
		else if (iteratingOverVisibleTerrainChunks)
		{
			visibleTerrainChunksToRemove.Add(chunk);
		}
		else
		{
			visibleTerrainChunks.Remove(chunk);
		}
	}
}

[Serializable]
public struct LODInfo
{
	[Range(0, MeshSettings.numSupportedLODs - 1)]
	public int lod;
	public float visibleDstThreshold;

	public float sqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
}