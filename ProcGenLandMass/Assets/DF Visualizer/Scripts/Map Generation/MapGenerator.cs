using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapGenerator : MonoBehaviour
{
	private const float viewerMoveThresholdForChunkUpdate = 25f;
	private const float sqrViewerMoveThresholdForChunkUpdate =
		viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;
	public WaterSettings waterSettings;

	public Transform viewer;
	public Material mapMaterial;

	[HideInInspector]
	public bool readyForPlayer;

	[HideInInspector]
	public float mapCenter;

	public event Action OnMapLoaded; 

	private Vector3 viewerPosition;
	private Vector3 viewerPositionOld;
	private float meshWorldSize;
	private int chunksVisibleInViewDst;

	private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	private HashSet<TerrainChunk> visibleTerrainChunks = new HashSet<TerrainChunk>();
	private HashSet<TerrainChunk> visibleTerrainChunksToRemove = new HashSet<TerrainChunk>();
	private bool iteratingOverVisibleTerrainChunks;

	private bool useExistingHeightMap;
	private HeightMap[,] existingHeightMaps;

	private void Start()
	{
		textureSettings.ApplyToMaterial(mapMaterial);
		textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
		
		float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

		if (GameManager.instance != null)
		{
			GameManager.instance.mapGenerator = this;
		}

		LoadNewMapFromImage(heightMapSettings.heightMapImage);
	}

	public void LoadNewMapFromImage(Texture2D image)
	{
		if (transform.childCount > 0)
		{
			foreach (Transform childTransform in transform.GetComponentsInChildren(typeof(Transform), true))
			{
				if (childTransform.gameObject != this.gameObject)
				{
					Destroy(childTransform.gameObject);
				}
			}
			terrainChunkDictionary.Clear();
			visibleTerrainChunks.Clear();
			visibleTerrainChunksToRemove.Clear();
		}
		existingHeightMaps = ImportHeightMap.ConvertToChunks(image, heightMapSettings, meshSettings);
		LoadChunks();
	}
	
	private async void LoadChunks()
	{
		for (int y = 0; y < existingHeightMaps.GetLength(1); y++)
		{
			for (int x = 0; x < existingHeightMaps.GetLength(0); x++)
			{
				TerrainChunk newChunk = new TerrainChunk(new Vector2(x, y), heightMapSettings, meshSettings, waterSettings,
					detailLevels, colliderLODIndex, transform, viewer, mapMaterial, existingHeightMaps);
				newChunk.Load();
				terrainChunkDictionary.Add(new Vector2(x, y), newChunk);
				newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
				await Task.Delay(1);
			}
		}
		OnMapLoaded?.Invoke();
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
