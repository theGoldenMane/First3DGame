using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public void Remove (Item item) {
		items.Remove(item);
		if (onItemChangedCallback != null) {
			onItemChangedCallback.Invoke();
		}
	}
}
