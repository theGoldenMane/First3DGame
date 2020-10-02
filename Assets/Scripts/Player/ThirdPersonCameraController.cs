using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
	public Transform player;
	private bool playerIsMoving = true;

	[Header("Movement")]
	public float mouseSensivity = 200f;
	private float turnHorizontal = 0.0f;
	private float turnVertical = 0.0f;

	[Header("Orbit")]
	public float rotationSpeed = 3.0f;

	[Header("Zoom")]
	public float minDistance = 5.0f;
	public float maxDistance = 15.0f;
	public float zoomSpeed = 10.0f;
	private float currentZoom;


	void OnEnable()
	{
		if (playerIsMoving) {
			/*
			turnHorizontal += Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
			turnVertical += Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;
			float clampedTurnVertical = Mathf.Clamp(-turnVertical, -90f, 90f);
			transform.eulerAngles = new Vector3(clampedTurnVertical, turnHorizontal , 0.0f);
			player.transform.Rotate(Vector3.up * turnHorizontal);
			*/

		} else {
			transform.LookAt(player);
			transform.RotateAround (player.position, Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed);

			currentZoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			Vector3 newPos = transform.position + transform.forward * currentZoom;
			float distance = Vector3.Distance(player.transform.position, newPos);

			if (distance > minDistance && distance < maxDistance) {
				transform.position += transform.forward * currentZoom;
			}
		}
	}
}
