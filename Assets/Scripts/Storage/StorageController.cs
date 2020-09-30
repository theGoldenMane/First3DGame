using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageController : MonoBehaviour
{
	public Animator animator;
	public GameObject storageInventory;

    public void OpenStorage() {
    	animator.SetBool("ChestOpen", true);
    	storageInventory.active = true;
    }

    public void CloseStorage() {
    	animator.SetBool("ChestOpen", false);
    	storageInventory.active = false;
    }
}
