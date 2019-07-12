using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
	
	private const float viewerMoveThresholdForChunkUpdate = 25f;
	private const float sqrViewerMoveThresholdForChunkUpdate =
		viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
	private const float colliderGenerationDistanceThreshold = 5f;

	public int colliderLODIndex;
	public LODInfo[] detailLevels;
	public static float maxViewDst;

	public Transform viewer;
	public Material mapMaterial;

	public static Vector3 viewerPosition;
	private Vector3 viewerPositionOld;
	private static MapGenerator mapGenerator;
	private int chunkSize;
	private int chunksVisibleInViewDst;

	private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	private static HashSet<TerrainChunk> visibleTerrainChunks = new HashSet<TerrainChunk>();
	private static HashSet<TerrainChunk> visibleTerrainChunksToRemove = new HashSet<TerrainChunk>();

	private void Start()
	{
		mapGenerator = FindObjectOfType<MapGenerator>();

		maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
		chunkSize = mapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
		
		UpdateVisibleChunks();
	}

	private void Update()
	{
		viewerPosition = new Vector3(viewer.position.x, viewer.position.z, 0) / mapGenerator.terrainData.uniformScale;

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
		foreach (TerrainChunk chunk in visibleTerrainChunks)
		{
			alreadyUpdatedChunkCoords.Add(chunk.coord);
			chunk.UpdateTerrainChunk();
		}

		foreach (TerrainChunk chunk in visibleTerrainChunksToRemove)
		{
			visibleTerrainChunks.Remove(chunk);
		}
		
		visibleTerrainChunksToRemove.Clear();
		
		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

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
						terrainChunkDictionary.Add(viewedChunkCoord, 
							new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, transform, mapMaterial));
					}
				}
			}
		}
	}

	public class TerrainChunk
	{
		public Vector2 coord;
		public static int newestChunkId;
		public int chunkId;
		
		private GameObject meshObject;
		private Vector2 position;
		private Bounds bounds;

		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;
		private MeshCollider meshCollider;

		private LODInfo[] detailLevels;
		private LODMesh[] lodMeshes;
		private int colliderLODIndex;

		private MapData mapData;
		private bool mapDataReceived;
		private int previousLodIndex = -1;
		private bool hasSetCollider;
		
		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
		{
			this.coord = coord;
			this.detailLevels = detailLevels;
			this.colliderLODIndex = colliderLODIndex;
			chunkId = newestChunkId++;
			
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);
			
			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = material;
			
			meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
			meshObject.transform.parent = parent;
			meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
			SetVisible(false);
			
			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++)
			{
				lodMeshes[i] = new LODMesh(detailLevels[i].lod);
				lodMeshes[i].updateCallback += UpdateTerrainChunk;
				if (i == colliderLODIndex)
				{
					lodMeshes[i].updateCallback += UpdateCollisionMesh;
				}
			}
			
			mapGenerator.RequestMapData(position, OnMapDataReceived);
		}

		private void OnMapDataReceived(MapData mapData)
		{
			this.mapData = mapData;
			mapDataReceived = true;

			UpdateTerrainChunk();
		}

		public void UpdateTerrainChunk()
		{
			if (mapDataReceived)
			{
				float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
				bool wasVisible = IsVisible();
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible)
				{
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++)
					{
						if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
						{
							lodIndex = i + 1;
						}
						else
						{
							break;
						}
					}

					if (lodIndex != previousLodIndex)
					{
						LODMesh lodMesh = lodMeshes[lodIndex];
						if (lodMesh.hasMesh)
						{
							previousLodIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
						}
						else if (!lodMesh.hasRequestedMesh)
						{
							lodMesh.RequestMesh(mapData);
						}
					}

					visibleTerrainChunks.Add(this);
				}

				if (wasVisible != visible)
				{
					if (visible)
					{
						visibleTerrainChunks.Add(this);
					}
					else
					{
						visibleTerrainChunksToRemove.Add(this);
					}
					SetVisible(visible);
				}
			}
		}

		public void UpdateCollisionMesh()
		{
			if (!hasSetCollider)
			{
				float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

				if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
				{
					if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
					{
						lodMeshes[colliderLODIndex].RequestMesh(mapData);
					}
				}
			
				if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
				{
					if (lodMeshes[colliderLODIndex].hasMesh)
					{
						meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
						hasSetCollider = true;
					}
				}
			}
		}

		public void SetVisible(bool visible)
		{
			meshObject.SetActive(visible);
		}

		public bool IsVisible()
		{
			return meshObject.activeSelf;
		}
		
	}

	private class LODMesh
	{
		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		private int lod;
		public event Action updateCallback;

		public LODMesh(int lod)
		{
			this.lod = lod;
		}

		private void OnMeshDataReceived(MeshData meshData)
		{
			mesh = meshData.CreateMesh();
			hasMesh = true;

			updateCallback();
		}
		
		public void RequestMesh(MapData mapData)
		{
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
		}

	}

	[Serializable]
	public struct LODInfo
	{
		[Range(0, MeshGenerator.numSupportedLODs - 1)]
		public int lod;
		public float visibleDstThreshold;

		public float sqrVisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
	}
	
}
