using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
	public CharacterController controller;

	public float walkSpeed = 12f;
	public float sprintSpeed = 24f;
	public float movementSpeed = 0;
	public float gravity = -25f;
	public float jumpHeight = 1f;
    public float crouchSpeed = 1000f;

	public Transform groundCheck;
	public float groundDistance = 0.4f;
	public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;
    private float x = 0f;
    private float z = 0f;
    private bool spaceAbove = true;

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

    [Header("Camera")]
    public Camera camera;
    //public Animator animator;

    // This cast rays against everything except layer 8.
    private int rayCastLayerMask = ~(1 << 8);

	private State state;
    private enum State {
        Sprint,
        Crouch,
        Slide,
        Normal, 
        GrapplingHookThrow,
        GrapplingHookFlying
    }

    void Awake() {
        state = State.Normal;
        movementSpeed = walkSpeed;
    }

    void Update()
    {
        switch(state) {
            default:
            case State.Normal: 
                HandleMovement();
                HandleCrouch();
                HandleSprint();
                HandleGrapplingHookShot();
                break;
            case State.GrapplingHookThrow:
                HandleMovement();
                HandleGrapplingHookThrow();
                break;
            case State.GrapplingHookFlying:
                HandleGrapplingHookMovement();
                break;
            case State.Sprint:
                HandleSprint();
                HandleSlide();
                HandleMovement();
                break;
            case State.Crouch:
                HandleCrouch();
                HandleMovement();
                break;
            case State.Slide:
                HandleSlide();
                break;
        }          
    }

    private void HandleMovement() {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if(isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }

        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * movementSpeed * Time.deltaTime);

        if(Input.GetButton("Jump") && isGrounded && (state == State.Normal || state == State.Sprint)) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleSprint() {
        if(Input.GetButtonDown("Sprint")) {
            movementSpeed = sprintSpeed;
            state = State.Sprint;
        } else if(Input.GetButtonUp("Sprint")) {
            movementSpeed = walkSpeed;
            state = State.Normal;
        }
    }

    private void HandleCrouch() {
        if (Input.GetButtonDown("Crouch") && state == State.Normal) {
            Crouch();
            state = State.Crouch;
        } else if ((Input.GetButtonUp("Crouch") && state == State.Crouch) || !spaceAbove) { 
            if(StandUp()) {
                state = State.Normal;
            }
        }
    }

    private void HandleSlide() {
        if(Input.GetButton("Crouch") && state == State.Sprint) {
            Crouch();
            state = State.Slide;
            slideX = Input.GetAxis("Horizontal");
            slideZ = Input.GetAxis("Vertical");
        }

        if(state == State.Slide) {
            slideTimer += Time.deltaTime;
            if(slideTimer < slideDuration) {
                x = slideX;
                z = slideZ;
            } else {
                if(StandUp()) {
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

    private void HandleGrapplingHookShot() {
    	if(Input.GetButtonDown("Throw Hook")) {           
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

        if(grapplingHookLineSize >= Vector3.Distance(transform.position, grapplingHookPosition)) {
            state = State.GrapplingHookFlying;
        }
    }

    private void HandleGrapplingHookMovement() {
        grapplingHookTransform.LookAt(grapplingHookPosition);

        Vector3 grapplingHookDirection = (grapplingHookPosition - transform.position).normalized;
        controller.Move(grapplingHookDirection * grapplingHookFlySpeed * Time.deltaTime);

        grapplingHookLineSize -= grapplingHookFlySpeed * Time.deltaTime;
        grapplingHookTransform.localScale = new Vector3(1, 1, grapplingHookLineSize);

        float reachedGrapplingHookPositionDistance = 3f;
        if(Vector3.Distance(transform.position, grapplingHookPosition) < reachedGrapplingHookPositionDistance) {
            state = State.Normal;
            velocity.y = -2f;
            //Set grappling line length to 0
            grapplingHookTransform.localScale = new Vector3(1, 1, 0f);
        }

        //If activated, cancels grapple
        /*if(Input.GetButtonDown("Throw Hook")) {
            HandleGrapplingHookShot();
        }*/
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        //Stop sliding on object collision
        if(state == State.Slide && hit.gameObject.name != "player" && hit.gameObject.name != "Plane") {
            StandUp();
            state = State.Normal;
        }
    }

    void OnGUI()
 	{
        Texture2D img;
        if(Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit raycastHit, maxGrappleDistance, rayCastLayerMask)) {
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
        if(spaceAbove) {
            camera.transform.position = new Vector3 (camera.transform.position.x, camera.transform.position.y + 1, camera.transform.position.z);
            controller.height = 2f;
            controller.center = new Vector3(0, 0, 0);
        }
        return spaceAbove;
    }
}
