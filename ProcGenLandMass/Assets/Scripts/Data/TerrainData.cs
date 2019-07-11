using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdateableData
{
	public float uniformScale = 5f;
	public bool useFlatShading;
	public bool useFalloff;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public float minHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
	public float maxHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
}
