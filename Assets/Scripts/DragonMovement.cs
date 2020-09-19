using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMovement : MonoBehaviour
{

	private Transform player;
	public float movementSpeed = 10f;
	private float minFollowDistance = 7.5f;

	void Start() {
		player = GameObject.FindWithTag("Player").transform;
	}

    void Update()
    {
    	transform.LookAt(player);

    	if(Vector3.Distance(player.position, transform.position) > minFollowDistance) {
    		transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
    	}
    }
}
