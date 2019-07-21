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

	public static event Action<Vector3> OnHighestPointChanged;
	public static event Action<Transform> OnViewerChanged; 
	
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

	private ImportHeightMap.HeightMapsData heightMapsData;

	private bool chunksNeedUpdating;

	private Vector2 highestPointChunkPos;
	private bool highestPointFound;

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
			GameManager.instance.OnGameStateChange += OnGameStateChanged;
		}

		LoadNewMapFromImage(heightMapSettings.heightMapImage);
	}

	private void ChangeViewer(Transform transform)
	{
		viewer = transform;
		chunksNeedUpdating = true;
		OnViewerChanged?.Invoke(transform);
	}

	private void OnGameStateChanged(UIState state)
	{
		switch (state)
		{
			case UIState.World:
				ChangeViewer(GameManager.instance.worldCamera.transform);
				break;
			case UIState.MainMenu:
				ChangeViewer(GameManager.instance.previewCamera.transform);
				break;
		}
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

		heightMapsData = ImportHeightMap.ConvertToChunks(image, heightMapSettings, meshSettings);
		existingHeightMaps = heightMapsData.heightMaps;
		highestPointChunkPos = heightMapsData.highestPointChunkCoord * meshWorldSize;
		OnHighestPointChanged?.Invoke(new Vector3(highestPointChunkPos.x, 180, highestPointChunkPos.y));
		SetupMapBounds();
		LoadChunks();
	}

	private void SetupMapBounds()
	{
		float mapWidth = heightMapsData.heightMaps.GetLength(0) * meshWorldSize;
		float mapHeight = heightMapsData.heightMaps.GetLength(0) * meshWorldSize;
		
		float colHeight = 512f;
		
		GameObject colliders = new GameObject("Colliders");
		colliders.transform.parent = transform;

		CreateCollider("West", new Vector3(1, colHeight, mapHeight), 
			new Vector3(-0.5f, colHeight * 0.5f, mapHeight * 0.5f));
		
		CreateCollider("East", new Vector3(1, colHeight, mapHeight),
			new Vector3(mapWidth + 0.5f, colHeight * 0.5f, mapHeight * 0.5f));
		
		CreateCollider("North", new Vector3(mapWidth, colHeight, 1),
			new Vector3(mapWidth * 0.5f, colHeight * 0.5f, mapHeight + 0.5f));
		
		CreateCollider("South", new Vector3(mapWidth, colHeight, 1),
			new Vector3(mapWidth * 0.5f, colHeight * 0.5f, -0.5f));
		
		CreateCollider("Top", new Vector3(mapWidth, 1, mapHeight),
			new Vector3(mapWidth * 0.5f, colHeight + 0.5f,  mapHeight * 0.5f));
		
		void CreateCollider(string name, Vector3 scale, Vector3 position)
		{
			GameObject col = new GameObject(name + "Collider");
			col.AddComponent<BoxCollider>();
			col.transform.parent = colliders.transform;
			col.transform.localScale = scale;
			col.transform.position = position;
		}
	}
	
	private void LoadChunks()
	{
		for (int y = 0; y < existingHeightMaps.GetLength(1); y++)
		{
			for (int x = 0; x < existingHeightMaps.GetLength(0); x++)
			{
				Vector2 coord = new Vector2(x, y);
				bool highPointOnChunk = coord == heightMapsData.highestPointChunkCoord;
				TerrainChunk newChunk = new TerrainChunk(coord, heightMapSettings, meshSettings, waterSettings,
					detailLevels, colliderLODIndex, transform, viewer, mapMaterial, existingHeightMaps, highPointOnChunk);
				newChunk.Load();
				terrainChunkDictionary.Add(new Vector2(x, y), newChunk);
				newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
				newChunk.onFoundHighestPoint += OnHighestPointChanged.Invoke;
			}
		}
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

				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord) || chunksNeedUpdating)
				{
					if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
					{
						terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					}
				}
			}
		}

		chunksNeedUpdating = false;
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
