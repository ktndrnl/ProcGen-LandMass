using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
	public Transform player;
	public TerrainGenerator terrainGenerator;

	public RawImage loadingScreen;
	public float loadingScreenFadeTime;
	private float timeOnLoadingScreen;

	private Rigidbody playerRb;
	private CapsuleCollider playerColl;

	private void Awake()
	{
		loadingScreen.color = Color.black;
	}

	private void Start()
	{
		playerRb = player.gameObject.GetComponent<Rigidbody>();
		playerColl = player.gameObject.GetComponent<CapsuleCollider>();
		StartCoroutine(WaitForMapLoad());
	}

	private IEnumerator WaitForMapLoad()
	{
		yield return new WaitForSeconds(0.5f);
		yield return new WaitUntil( () => terrainGenerator.readyForPlayer);
		PlacePlayerOnGround();
		StartCoroutine(FadeOutLoadingScreen());
	}

	private void PlacePlayerOnGround()
	{
		RaycastHit hit;
		Ray ray = new Ray(playerRb.position, Vector3.down);
		Physics.Raycast(ray, out hit);
		playerRb.MovePosition(hit.point);
	}

	private IEnumerator FadeOutLoadingScreen()
	{
		while (timeOnLoadingScreen <= loadingScreenFadeTime)
		{
			timeOnLoadingScreen += Time.deltaTime;
			loadingScreen.color = 
				Color.Lerp(Color.black, Color.clear, timeOnLoadingScreen * (1 / loadingScreenFadeTime));
			yield return null;
		}
		Destroy(gameObject);
	}
}
