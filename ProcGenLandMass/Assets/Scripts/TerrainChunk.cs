using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class TerrainChunk
{
	private const float colliderGenerationDistanceThreshold = 5f;
	public const float waterPlaneScaleMultiplier = 2.94f;

	public event Action<TerrainChunk, bool> onVisibilityChanged;
	
	public Vector2 coord;

	private GameObject meshObject;
	private Vector2 sampleCenter;
	private Bounds bounds;

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private MeshCollider meshCollider;
	
	private GameObject waterObject;
	private MeshRenderer waterMeshRenderer;
	private MeshFilter waterMeshFilter;

	private LODInfo[] detailLevels;
	private LODMesh[] lodMeshes;
	private int colliderLODIndex;

	private HeightMap heightMap;
	private bool heightMapReceived;
	private int previousLodIndex = -1;
	private bool hasSetCollider;
	private float maxViewDst;

	private HeightMapSettings heightMapSettings;
	private MeshSettings meshSettings;
	private WaterSettings waterSettings;
	private Transform viewer;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, WaterSettings waterSettings,
						LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
	{
		this.coord = coord;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.waterSettings = waterSettings;
		this.viewer = viewer;

		sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize;
		bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;
		
		meshObject.transform.position = new Vector3(position.x, 0, position.y);
		meshObject.transform.parent = parent;
		
		waterObject = GameObject.Instantiate(waterSettings.waterPlanePrefab, meshObject.transform);
		//TODO: way to set water height based on terrain texture settings | 1.78f
		waterObject.transform.localPosition = new Vector3(0, waterSettings.waterHeight , 0);
		waterObject.transform.localScale *= waterSettings.waterPlaneScaleMultiplier;

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

		maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
	}

	public void Load()
	{
		ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(
			meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine,
			heightMapSettings, sampleCenter), OnHeightMapReceived);
	}

	private void OnHeightMapReceived(object heightMapObject)
	{
		heightMap = (HeightMap) heightMapObject;
		heightMapReceived = true;

		UpdateTerrainChunk();
	}

	private Vector2 viewerPosition => new Vector2(viewer.position.x, viewer.position.z);

	public void UpdateTerrainChunk()
	{
		if (heightMapReceived)
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
						lodMesh.RequestMesh(heightMap, meshSettings);
					}

					waterObject.SetActive(detailLevels[lodIndex].waterVisible);
				}
			}

			if (wasVisible != visible)
			{
				SetVisible(visible);
				onVisibilityChanged?.Invoke(this, visible);
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
					lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
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

		private void OnMeshDataReceived(object meshDataObject)
		{
			mesh = ((MeshData) meshDataObject).CreateMesh();
			hasMesh = true;

			updateCallback();
		}

		public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
		{
			hasRequestedMesh = true;
			ThreadedDataRequester.RequestData(
				() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod),
				OnMeshDataReceived);
		}
	}
}
