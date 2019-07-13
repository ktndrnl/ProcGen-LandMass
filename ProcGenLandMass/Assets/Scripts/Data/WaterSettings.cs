using UnityEngine;

[CreateAssetMenu]
public class WaterSettings : UpdateableData
{
	// 100x100 = 2.94f, 20x20 = 14.7f
	public readonly float waterPlaneScaleMultiplier = 14.7f;

	public GameObject waterPlanePrefab;
	public float waterHeight = 0.6f;
}
