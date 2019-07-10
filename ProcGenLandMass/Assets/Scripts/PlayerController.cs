using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float speed = 10f;
	public float lookSpeedV = 10f;
	public float lookSpeedH = 10f;

	public Camera cam;

	private Rigidbody rb;
	
	private Vector3 inputDirection = Vector3.zero;
	private float mouseLookDirectionH;
	private float mouseLookDirectionV;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		rb.MovePosition(transform.position += (speed * Time.fixedDeltaTime * inputDirection));
		cam.transform.Rotate(mouseLookDirectionV, 0, 0);
		Quaternion rot = Quaternion.Euler(Mathf.Clamp(cam.transform.rotation.eulerAngles.x , 90f, -90f),
			cam.transform.rotation.eulerAngles.y, cam.transform.rotation.eulerAngles.z);
		cam.transform.rotation = rot;
		transform.RotateAround(transform.position, Vector3.up , mouseLookDirectionH);
	}

	void Update()
    {
        inputDirection = 
			new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
		mouseLookDirectionH = Input.GetAxisRaw("Mouse X") * lookSpeedH * Time.deltaTime;
		mouseLookDirectionV = -Input.GetAxisRaw("Mouse Y") * lookSpeedV * Time.deltaTime;
	}
}
