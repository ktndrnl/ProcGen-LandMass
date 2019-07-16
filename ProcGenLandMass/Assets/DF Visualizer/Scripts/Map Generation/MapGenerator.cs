using System.Collections.Generic;
using UnityEngine;

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
	
	
}
