using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdateableData
{
	public const int numSupportedLODs = 5;
	public const int numSupportedChunkSizes = 9;
	public const int numSupportedFlatshadedChunkSizes = 3;
	public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
	public static readonly int[] supportedFlatshadedChunkSizes = {48, 72, 96};
	
	public float meshScale = 5f;
	public bool useFlatShading;
	
	[Range(0, numSupportedChunkSizes - 1)]
	public int chunkSizeIndex;
	[Range(0, numSupportedFlatshadedChunkSizes - 1)]
	public int flatshadedChunkSizeIndex;
	
	// num vertices per line of mesh rendered at LOD = 0
	// Includes the 2 extra vert that are excluded from final mesh, but used for calculating normals.
	public int numVerticesPerLine => supportedChunkSizes[useFlatShading ? flatshadedChunkSizeIndex : chunkSizeIndex] + 5;

	public float meshWorldSize => (numVerticesPerLine - 3) * meshScale;
}
