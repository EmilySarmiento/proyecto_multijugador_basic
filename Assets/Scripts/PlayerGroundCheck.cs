using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
	PlayerController playerController;

	void Awake()
	{
		playerController = GetComponentInParent<PlayerController>();
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject == playerController.gameObject)
            //Debug.Log("El jugador esta en el piso");
			return;

		playerController.SetGroundedState(true);
	}

	void OnTriggerExit(Collider other)
	{
		if(other.gameObject == playerController.gameObject)
            //Debug.Log("No esta en el piso");
			return;

		playerController.SetGroundedState(false);
	}

	void OnTriggerStay(Collider other)
	{
		if(other.gameObject == playerController.gameObject)
            //Debug.Log("El jugador esta en el piso");
			return;

		playerController.SetGroundedState(true);
	}
}