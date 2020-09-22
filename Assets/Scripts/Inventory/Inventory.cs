using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;

	public delegate void OnItemChange();
	public OnItemChange onItemChangedCallback;

	public int space = 25;
	public List<Item> items = new List<Item>();


	void Awake ()
	{
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else if (instance != this)
		{
			Destroy (gameObject);
		}
	}

	void Update () {
		if (GlobalGameManager.instance.inInventory) {
			if (EventSystem.current.currentSelectedGameObject && items.Count > 0) {
				if (Input.GetKeyDown(KeyCode.O)) {
					int slotIndex = EventSystem.current.currentSelectedGameObject.transform.parent.transform.GetSiblingIndex();
					Drop(items[slotIndex]);
				} else if (Input.GetKeyDown(KeyCode.X)) {
					int slotIndex = EventSystem.current.currentSelectedGameObject.transform.parent.transform.GetSiblingIndex();
					Destroy(items[slotIndex]);
				}
			}
		}
	}

	public bool Add (Item item) {
		if (items.Count < space) {
			items.Add(item);
			if (onItemChangedCallback != null) {
				onItemChangedCallback.Invoke();
			}
			return true;
		}
		return false;
	}

	public void Drop (Item item) {
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		int slotIndex = EventSystem.current.currentSelectedGameObject.transform.parent.transform.GetSiblingIndex();
		GameObject prefab = items[slotIndex].prefab;

		Vector3 spawnPos = player.transform.position + player.transform.forward * 2f;
		Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 5f, (1<<9));
		spawnPos.y = hit.point.y + items[slotIndex].prefab.transform.localScale.y / 2 + 0.1f;
						
		Instantiate(prefab, spawnPos, Quaternion.identity);
		items.Remove(item);

		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void Destroy (Item item) {
		items.Remove(item);
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}
}
