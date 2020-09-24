using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
	public static Inventory instance;

	public delegate void OnItemChange();
	public OnItemChange onItemChangedCallback;

	public Item[] items;

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

	void Start() {
		items = new Item[25];
	}

	void Update () {
		if (GlobalGameManager.instance.inInventory) {
			if (EventSystem.current.currentSelectedGameObject && items.Length > 0) {
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
		// Get first empty array slot
		int indexOfEmpty = -1;
		for (int i = 0; i < items.Length; i++) {
			if (items[i] == null) {
				indexOfEmpty = i;
				break;
			}
		}

		// If empty slot exists, add item to it
		if (indexOfEmpty > -1) {
			items[indexOfEmpty] = item;
			if (onItemChangedCallback != null) {
				onItemChangedCallback.Invoke();
			}
			return true;
		} else {
			Debug.LogWarning("No space in Inventory");
		}

		return false;
	}

	public void Drop (Item item) {
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		int slotIndex = EventSystem.current.currentSelectedGameObject.transform.parent.transform.GetSiblingIndex();
		GameObject prefab = items[slotIndex].prefab;

		Vector3 spawnPos = player.transform.position + player.transform.forward * 2f;
		Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 5f, (1 << 9));
		spawnPos.y = hit.point.y + items[slotIndex].prefab.transform.localScale.y / 2 + 0.1f;

		Instantiate(prefab, spawnPos, Quaternion.identity);

		int index = System.Array.IndexOf(items, item);
		if (index != -1) items[index] = null;

		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}

	public void Destroy (Item item) {
		int index = System.Array.IndexOf(items, item);
		if (index != -1) items[index] = null;
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}
}
