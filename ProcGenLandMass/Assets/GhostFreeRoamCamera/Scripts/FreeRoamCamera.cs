using System;
using Unity.Mathematics;
using UnityEngine;

public class FreeRoamCamera : MonoBehaviour
{
	public float initialSpeed = 10f;
    public float increaseSpeed = 1.25f;

	public float maxSpeed = 100f;

    public bool allowMovement = true;
    public bool allowRotation = true;

    public KeyCode forwardButton = KeyCode.W;
    public KeyCode backwardButton = KeyCode.S;
    public KeyCode rightButton = KeyCode.D;
    public KeyCode leftButton = KeyCode.A;

    public float cursorSensitivity = 0.025f;
    public bool cursorToggleAllowed = true;
    public KeyCode cursorToggleButton = KeyCode.Escape;

	private Rigidbody rb;
	private Vector3 eulerAngles;

    private float currentSpeed = 0f;
	private Vector3 deltaPosition;
    private bool moving = false;
	private bool lastMoving;
    private bool togglePressed = false;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		GameManager.instance.OnGameStateChange += OnGameStateChanged;
	}

	private void FixedUpdate()
	{
		if (moving)
		{
			if (moving != lastMoving)
				currentSpeed = initialSpeed;

			//transform.position += deltaPosition * currentSpeed * Time.deltaTime;
			//rb.MovePosition(rb.position += currentSpeed * Time.deltaTime * deltaPosition);
			//rb.AddForce(currentSpeed * Time.deltaTime * deltaPosition.normalized, ForceMode.VelocityChange);
			rb.velocity += maxSpeed * Time.deltaTime * deltaPosition.normalized;
			if (rb.velocity.magnitude >= maxSpeed)
			{
				rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
			}
		}
		else
		{
			currentSpeed = 0f;
			rb.AddForce(rb.velocity * -0.5f, ForceMode.VelocityChange);
		}

		transform.eulerAngles = eulerAngles;

		//transform.rotation = inputCamera.rotation;

		/*
		if (allowRotation)
		{
			// eulerAngles = transform.eulerAngles;
			// eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * cursorSensitivity;
			// eulerAngles.y += Input.GetAxis("Mouse X") * 359f * cursorSensitivity;
			//rb.rotation = Quaternion.Euler(eulerAngles);
			//rb.MoveRotation(quaternion.Euler(eulerAngles));
		}
		*/

		// float turnH = Input.GetAxis("Horizontal");
		// rb.AddTorque(turnH * cursorSensitivity * transform.up);
		// float turnV = Input.GetAxis("Vertical");
		// rb.AddTorque(turnV * cursorSensitivity * transform.right);
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
        }

		if (allowRotation)
        {
            eulerAngles = transform.eulerAngles;
            eulerAngles.x += -Input.GetAxis("Mouse Y") * 359f * cursorSensitivity;
            eulerAngles.y += Input.GetAxis("Mouse X") * 359f * cursorSensitivity;
            //transform.eulerAngles = eulerAngles;
			//rb.MoveRotation(quaternion.Euler(eulerAngles));
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
