﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class Item : ScriptableObject
{
	public int id;
	new public string name = "New Item";
	public Sprite icon = null;
	public GameObject prefab = null;
	public int maxStackSize = 1;
}