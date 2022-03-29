using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
	public Item item;
	public int amount;

	public void PickUp()
	{
		int addRequest = Inventory.instance.Add(item, amount);
		if (addRequest == 0) {
			Destroy(gameObject);
		} else {
			amount = addRequest;
		}
	}
}
