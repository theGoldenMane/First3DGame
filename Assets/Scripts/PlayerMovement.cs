﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

	[Header("Movement")]
	public CharacterController controller;

	public float walkSpeed = 12f;
	public float sprintSpeed = 24f;
	public float movementSpeed = 0;
	public float crouchSpeed = 1000f;
	public float climbSpeed = 3f;
	public float gravity = -25f;
	public float jumpHeight = 1f;

	public Transform groundCheck;
	public float groundDistance = 0.4f;
	private LayerMask groundMask = ~(1 << 10);

	private Vector3 velocity;
	private bool isGrounded;
	private float x = 0f;
	private float z = 0f;
	private bool spaceAbove = true;
	private Vector3 move;
	private float jumpVerticalDirection;

	[Header("Camera")]
	public Camera camera;
	public float mouseSensivity = 200f;
	float xRotation = 0f;

	[Header("Running Slide")]
	public float slideDuration = 1f;
	private float slideTimer = 0f;
	private float slideX;
	private float slideZ;

	[Header("Crosshair")]
	public Texture2D crosshairImageDefault;
	public Texture2D crosshairImageOnObject;
	public int crosshairImgWidth = 100;
	public int crosshairImgHeight = 50;

	[Header("Grappling Hook")]
	public float grapplingHookFlySpeed = 50f;
	public int maxGrappleDistance = 40;
	public Transform grapplingHookTransform;
	public float grapplingHookThrowSpeed = 90;
	private Vector3 grapplingHookPosition;
	private float grapplingHookLineSize = 0f;

	[Header("Ledge Climb")]
	private int ledgeClimbLayer = 1 << 11;
	public float ledgeClimbDistance = 1f;
	public float ledgeClimbSpeed = 8f;
	private Vector3 ledgeClimbObjectHitPoint;
	private float ledgeClimbTargetHeight;
	private Vector3 forwardDirection;
	private Vector3 upDirection;
	private Vector3 ledgeClimbUpPos;
	private LedgeClimbState ledgeState;
	private enum LedgeClimbState {
		ClimbUp,
		GoForward,
		None
	}

	[Header("Glider")]
	public float glideHorizontalSpeed = 8f;
	public float glideVerticalSpeed = -2f;

	// This cast rays against everything except layer 10
	private int rayCastLayerMask = ~(1 << 10);

	private State state;
	private enum State {
		Jump,
		Sprint,
		Crouch,
		Slide,
		Climb,
		LedgeClimb,
		Glide,
		Normal,
		GrapplingHookThrow,
		GrapplingHookFlying
	}

	void Awake() {
		state = State.Normal;
		ledgeState = LedgeClimbState.None;
		movementSpeed = walkSpeed;
	}

	void Start()
	{
		//Lock Cursor
		Cursor.lockState = CursorLockMode.Locked;
	}


	void Update()
	{
		switch (state) {
		default:
		case State.Normal:
			HandleCamera();
			HandleMovement();
			HandleCrouch();
			HandleSprint();
			HandleGlideStart();
			HandleGrapplingHookShot();
			HandleJump();
			DetectClimb();
			break;
		case State.Jump:
			HandleCamera();
			HandleMovement();
			HandleJump();
			DetectClimb();
			CheckLedgeInRange();
			break;
		case State.Sprint:
			HandleMovement();
			HandleCamera();
			HandleSprint();
			HandleSlide();
			HandleGrapplingHookShot();
			HandleJump();
			break;
		case State.Crouch:
			HandleCamera();
			HandleCrouch();
			HandleMovement();
			break;
		case State.Slide:
			HandleCamera();
			HandleSlide();
			break;
		case State.Climb:
			HandleCamera();
			HandleClimb();
			CheckLedgeInRange();
			break;
		case State.LedgeClimb:
			HandleLedgeClimb();
			break;
		case State.Glide:
			HandleGlideMovement();
			HandleCamera();
			DetectClimb();
			break;
		case State.GrapplingHookThrow:
			HandleCamera();
			HandleMovement();
			HandleGrapplingHookThrow();
			break;
		case State.GrapplingHookFlying:
			HandleCamera();
			HandleGrapplingHookMovement();
			CheckLedgeInRange();
			break;
		}
	}

	private void HandleCamera() {
		float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);

		camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		transform.Rotate(Vector3.up * mouseX);
	}

	private void HandleMovement() {
		x = Input.GetAxisRaw("Horizontal");
		z = Input.GetAxisRaw("Vertical");
		if (state == State.Jump) {
			z = jumpVerticalDirection;
		}
		move = transform.right * x + transform.forward * z;
		move.Normalize();
		controller.Move(move * movementSpeed * Time.deltaTime);
	}

	private void HandleJump() {
		//Velocity needs to be reset otherwise force/speed to ground would increase with every jump
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
		if (isGrounded && velocity.y < 0) {
			velocity.y = -2f;
		}


		if (state == State.Jump && isGrounded && velocity.y < 0) {
			movementSpeed = walkSpeed;
			state = State.Normal;
		}

		if (state == State.Jump && !isGrounded) {
			//DetectLedgeClimb();
		}

		if (Input.GetButtonDown("Jump") && isGrounded) {
			//DetectLedgeClimb();
			if (state != State.LedgeClimb) {
				jumpVerticalDirection = Input.GetAxisRaw("Vertical");
				state = State.Jump;
				velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			}
		}

		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}

	private void HandleSprint() {
		// Sprint if player isn't moving backwards
		if (Input.GetButton("Sprint") && !Input.GetKey(KeyCode.S) && state != State.Jump) {
			movementSpeed = sprintSpeed;
			state = State.Sprint;
		} else if (state == State.Sprint) {
			movementSpeed = walkSpeed;
			state = State.Normal;
		}
	}

	private void HandleCrouch() {
		if (Input.GetButtonDown("Crouch") && state == State.Normal) {
			Crouch();
			state = State.Crouch;
		} else if ((Input.GetButtonUp("Crouch") && state == State.Crouch) || !spaceAbove) {
			if (StandUp()) {
				state = State.Normal;
			}
		}
	}

	private void HandleSlide() {
		if (Input.GetButton("Crouch") && state == State.Sprint) {
			Crouch();
			state = State.Slide;
			slideX = Input.GetAxis("Horizontal");
			slideZ = Input.GetAxis("Vertical");
		}

		if (state == State.Slide) {
			slideTimer += Time.deltaTime;
			if (slideTimer < slideDuration) {
				x = slideX;
				z = slideZ;
			} else {
				if (StandUp()) {
					slideTimer = 0f;
					state = State.Normal;
				} else {
					x = Input.GetAxis("Horizontal");
					z = Input.GetAxis("Vertical");
				}
			}

			Vector3 move = transform.right * x + transform.forward * z;
			controller.Move(move * movementSpeed * Time.deltaTime);
		}
	}

	private void DetectClimb() {
		if ((Input.GetButtonDown("Jump") && !isGrounded && state == State.Normal) || (state == State.Jump && !isGrounded) || state == State.Glide) {
			if (Physics.Raycast(transform.position, transform.forward, out RaycastHit climbableObjectHit, ledgeClimbDistance, rayCastLayerMask)) {
				if (climbableObjectHit.transform.tag == "Climbable-Object" && z > 0) {
					movementSpeed = climbSpeed;
					state = State.Climb;
				}
			}
		}
	}

	private void HandleClimb() {
		velocity.y = 0;
		x = Input.GetAxisRaw("Horizontal");
		z = Input.GetAxisRaw("Vertical");
		move = transform.right * x + transform.up * z;
		move.Normalize();
		controller.Move(move * movementSpeed * Time.deltaTime);

		if (Input.GetButtonDown("Jump")) {
			velocity.y = -2f;
			movementSpeed = walkSpeed;
			state = State.Normal;
		}

		if (isGrounded) {
			movementSpeed = walkSpeed;
			state = State.Normal;
		}
	}

	private void CheckLedgeInRange() {
		// Check if player is 1m from ledge, climb up (calc: climbObj.position.y + climbObj.height/2 - hitLocation.y - PlayerSize/2 <= 1)
		if (z > 0) {
			if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, ledgeClimbDistance, ledgeClimbLayer)) {
				if (hit.transform.localScale.y / 2 + hit.transform.position.y - hit.point.y - transform.localScale.y / 2 <= 2) {
					ledgeClimbObjectHitPoint = hit.point;
					ledgeClimbTargetHeight = hit.transform.localScale.y / 2 +  hit.transform.position.y + transform.localScale.y + 0.2f;
					forwardDirection = (ledgeClimbObjectHitPoint - transform.position).normalized;
					upDirection = new Vector3(0, ledgeClimbTargetHeight, 0).normalized;
					state = State.LedgeClimb;
					ledgeState = LedgeClimbState.ClimbUp;
				}
			}
		}
	}

	private void HandleLedgeClimb() {
		switch (ledgeState) {
		default:
		case LedgeClimbState.ClimbUp:
			controller.Move(upDirection.normalized * ledgeClimbSpeed * Time.deltaTime);
			if (transform.position.y >= ledgeClimbTargetHeight) {
				ledgeClimbUpPos = transform.position;
				ledgeState = LedgeClimbState.GoForward;
			}
			break;
		case LedgeClimbState.GoForward:
			controller.Move(transform.forward * 1 * ledgeClimbSpeed * Time.deltaTime);
			if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 2f, ledgeClimbLayer)) {
				movementSpeed = walkSpeed;
				ledgeState = LedgeClimbState.None;
				state = State.Normal;
			}
			break;
		}
	}

	private void HandleGlideStart() {
		if (Input.GetButtonDown("Glide")) {
			movementSpeed = glideHorizontalSpeed;
			velocity.y = glideVerticalSpeed;
			state = State.Glide;
		}
	}

	private void HandleGlideMovement() {
		x = Input.GetAxis("Horizontal");
		z = Input.GetAxis("Vertical");
		Vector3 move = transform.right * x + transform.forward * z;
		controller.Move(move * movementSpeed * Time.deltaTime);
		controller.Move(velocity * Time.deltaTime);

		//Stop glide if canceled with button press or by landing
		if (Input.GetButtonDown("Glide") || Physics.CheckSphere(groundCheck.position, groundDistance, groundMask)) {
			state = State.Normal;
		}
	}

	private void HandleGrapplingHookShot() {
		if (Input.GetButtonDown("Throw Hook")) {
			if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit raycastHit, maxGrappleDistance, rayCastLayerMask))
			{
				//animator.SetBool("throwHook", true);
				grapplingHookPosition = raycastHit.point;
				grapplingHookLineSize = 0f;
				state = State.GrapplingHookThrow;
				//Debug.DrawRay(camera.transform.position, camera.transform.forward * raycastHit.distance, Color.red, 30);
			}
		}
	}

	private void HandleGrapplingHookThrow() {
		grapplingHookTransform.LookAt(grapplingHookPosition);

		grapplingHookLineSize += grapplingHookThrowSpeed * Time.deltaTime;
		grapplingHookTransform.localScale = new Vector3(1, 1, grapplingHookLineSize);

		if (grapplingHookLineSize >= Vector3.Distance(transform.position, grapplingHookPosition)) {
			state = State.GrapplingHookFlying;
		}
	}

	private void HandleGrapplingHookMovement() {
		grapplingHookTransform.LookAt(grapplingHookPosition);

		Vector3 grapplingHookDirection = (grapplingHookPosition - transform.position).normalized;
		controller.Move(grapplingHookDirection * grapplingHookFlySpeed * Time.deltaTime);

		grapplingHookLineSize -= grapplingHookFlySpeed * Time.deltaTime;
		grapplingHookTransform.localScale = new Vector3(1, 1, grapplingHookLineSize);

		float reachedGrapplingHookPositionDistance = 1.1f;
		if (Vector3.Distance(transform.position, grapplingHookPosition) < reachedGrapplingHookPositionDistance) {
			state = State.Normal;
			velocity.y = -2f;
			//Set grappling line length to 0
			grapplingHookTransform.localScale = new Vector3(0f, 0f, 0f);
			z = 1;
		}

		//If activated, cancels grapple
		/*if(Input.GetButtonDown("Throw Hook")) {
		    HandleGrapplingHookShot();
		}*/
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		//Stop sliding on object collision
		if (state == State.Slide && hit.gameObject.name != "player" && hit.gameObject.name != "Plane") {
			//StandUp();
			//state = State.Normal;
		}
	}

	void OnGUI()
	{
		Texture2D img;
		if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit raycastHit, maxGrappleDistance, rayCastLayerMask)) {
			img = crosshairImageDefault;
		} else {
			img = crosshairImageOnObject;
		}
		float xMin = (Screen.width / 2) - (crosshairImgWidth / 2);
		float yMin = (Screen.height / 2) - (crosshairImgHeight / 2);
		GUI.DrawTexture(new Rect(xMin, yMin, crosshairImgWidth, crosshairImgHeight), img);
	}

	private void Crouch() {
		camera.transform.position = new Vector3 (camera.transform.position.x, camera.transform.position.y - 1, camera.transform.position.z);
		controller.height = 1f;
		controller.center = new Vector3 (0, -0.5f, 0);
		//camera.transform.position =Vector3.Lerp (defaultCameraPos.position, crouchCameraPos.position, Time.deltaTime);  defaultCameraPos, crouchCameraPos add to inspector
	}

	private bool StandUp() {
		spaceAbove = !Physics.Raycast(transform.position, Vector3.up, 1.3f);
		if (spaceAbove) {
			camera.transform.position = new Vector3 (camera.transform.position.x, camera.transform.position.y + 1, camera.transform.position.z);
			controller.height = 2f;
			controller.center = new Vector3(0, 0, 0);
		}
		return spaceAbove;
	}
}