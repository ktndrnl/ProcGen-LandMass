using System;
using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdateableData
{
	public NoiseSettings noiseSettings;
	
	public bool useFalloff;
	public bool useExistingHeightMap;
	public Texture2D heightMapImage;

	public float heightMultiplier;
	public AnimationCurve heightCurve;

	public float minHeight => heightMultiplier * heightCurve.Evaluate(0);
	public float maxHeight => heightMultiplier * heightCurve.Evaluate(1);

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		noiseSettings.ValidateValues();
		base.OnValidate();
	}
	#endif
}
