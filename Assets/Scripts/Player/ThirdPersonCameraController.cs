using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
	public Transform player;
	public bool playerIsMoving = false;

	[Header("Movement")]
	public float mouseSensivity = 200f;
	private float turnHorizontal = 0.0f;
	private float turnVertical = 0.0f;
	private float xRotation = 0.0f;

	[Header("Orbit")]
	public float rotationSpeed = 4f;

	[Header("Zoom")]
	public float minDistance = 5.0f;
	public float maxDistance = 15.0f;
	public float zoomSpeed = 10.0f;
	private float currentZoom;


	private float lerpTimer = 0f;
	public float smooth = 0.5f;

	/*
	public float height = 1f;
	public float distance = 2f;
	private Vector3 offsetX;
	private Vector3 offsetY;
	*/

	/*void Start () {
		offsetY = new Vector3 (0, 0, distance);
	}*/

	void OnEnable()
	{
		if (playerIsMoving) {
			
			float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
			float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;

			xRotation -= mouseY;
			xRotation = Mathf.Clamp(xRotation, -90f, 90f);

			transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
			//Rotation up/down missing
			//offsetY = Quaternion.AngleAxis (Input.GetAxis("Mouse Y") * turnSpeed, Vector3.right) * offsetY;

			player.transform.Rotate(Vector3.up * mouseX);

			// Zoom
			currentZoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			Vector3 newPos = transform.position + transform.forward * currentZoom;
			float distance = Vector3.Distance(player.transform.position, newPos);
			if (distance > minDistance && distance < maxDistance) {
				transform.position += transform.forward * currentZoom;
			}

		} else {
			// Orbit
			//Rotation up/down missing
			//transform.RotateAround (player.position, Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed);
			//transform.RotateAround (player.position, new Vector3 (1, 0, 0), Input.GetAxis("Mouse Y") * rotationSpeed);
			//transform.LookAt(player);


			
		}
	}
}
