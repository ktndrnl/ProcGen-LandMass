using System;
using Unity.Mathematics;
using UnityEngine;

public class FreeRoamCamera : MonoBehaviour
{
	public Vector3 startingPosition;
	public Vector3 startingRotation;
	
	public float initialSpeed = 10f;
    public float increaseSpeed = 1.25f;

	public float maxSpeed = 100f;
	private float boostMaxSpeed;
	public float speedBoostMultiplier = 2f;
	
	private float currentMaxSpeed;
	private float currentSpeedMultiplier = 1f;

    public bool allowMovement = true;
    public bool allowRotation = true;

    public KeyCode forwardButton = KeyCode.W;
    public KeyCode backwardButton = KeyCode.S;
    public KeyCode rightButton = KeyCode.D;
    public KeyCode leftButton = KeyCode.A;
	public KeyCode upButton = KeyCode.LeftShift;
	public KeyCode downButton = KeyCode.LeftControl;
	public KeyCode boostButton = KeyCode.LeftShift;

    public float cursorSensitivity = 0.025f;

	private Rigidbody rb;
	private Vector3 eulerAngles;

    private float currentSpeed = 0f;
	private Vector3 deltaPosition;
    private bool moving = false;
	private bool lastMoving;
    private bool togglePressed = false;

	private float rotX;
	private float rotY;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		GameManager.instance.OnGameStateChange += OnGameStateChanged;
		GameManager.instance.OnStartGame += ResetCameraPosition;

		boostMaxSpeed = maxSpeed * speedBoostMultiplier;
	}

	private void ResetCameraPosition()
	{
		transform.position = startingPosition;
		transform.rotation = Quaternion.Euler(startingRotation);
	}

	private void FixedUpdate()
	{
		if (moving)
		{
			if (moving != lastMoving)
				currentSpeed = initialSpeed;
			
			rb.velocity += currentMaxSpeed * Time.deltaTime * currentSpeedMultiplier * deltaPosition.normalized;
			if (rb.velocity.magnitude >= currentMaxSpeed)
			{
				rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
			}
		}
		else
		{
			currentSpeed = 0f;
			rb.AddForce(rb.velocity * -0.5f, ForceMode.VelocityChange);
		}
	}

	private void Update()
    {
		if (allowMovement)
        {
            lastMoving = moving;
            deltaPosition = Vector3.zero;

            if (moving)
                currentSpeed += increaseSpeed * Time.deltaTime;

            moving = false;

            CheckMove(forwardButton, ref deltaPosition, transform.forward);
            CheckMove(backwardButton, ref deltaPosition, -transform.forward);
            CheckMove(rightButton, ref deltaPosition, transform.right);
            CheckMove(leftButton, ref deltaPosition, -transform.right); 
			CheckMove(upButton, ref deltaPosition, Vector3.up);
			CheckMove(downButton, ref deltaPosition, Vector3.down);

			if (Input.GetKey(boostButton))
			{
				currentMaxSpeed = boostMaxSpeed;
				currentSpeedMultiplier = speedBoostMultiplier;
			}
			else
			{
				currentMaxSpeed = maxSpeed;
				currentSpeedMultiplier = 1f;
			}
        }

		if (allowRotation)
        {
			rotX += Input.GetAxis("Mouse X") * cursorSensitivity;
			rotY += Input.GetAxis("Mouse Y") * cursorSensitivity;
			rotY = Mathf.Clamp(rotY, -90f, 90f);
			transform.rotation = Quaternion.Euler(-rotY, rotX, 0f);
		}
	}
	
	private void OnGameStateChanged(UIState state)
	{
		switch (state)
		{
			case UIState.MainMenu:
				DisableInput();
				break;
			case UIState.PauseMenu:
				DisableInput();
				break;
			case UIState.World:
				EnableInput();
				break;
		}
	}

	private void DisableInput()
	{
		allowMovement = false;
		allowRotation = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void EnableInput()
	{
		allowMovement = true;
		allowRotation = true;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

    private void CheckMove(KeyCode keyCode, ref Vector3 deltaPosition, Vector3 directionVector)
    {
        if (Input.GetKey(keyCode))
        {
            moving = true;
            deltaPosition += directionVector;
        }
    }
}
