﻿using System;
using UnityEngine;

public class UpdateableData : ScriptableObject
{
	public event Action OnValuesUpdated;
	public bool autoUpdate;

	protected virtual void OnValidate()
	{
		if (autoUpdate)
		{
			NotifyOfUpdatedValues();
		}
	}

	public void NotifyOfUpdatedValues()
	{
		OnValuesUpdated?.Invoke();
	}
}
