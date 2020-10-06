﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour,
   IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
   public Image icon;
   public Text amount;
   private Item item;

   private Transform originalParent;
   private int originalIndex;
   private GameObject clone;

   private float left = 0f;
   private float right = 0f;
   private float top = 0f;
   private float bottom = 0f;
   private bool dragging = false;

   public void AddItem(Item newItem, int newAmount) {
      item = newItem;
      icon.sprite = item.icon;
      icon.enabled = true;
      amount.text = newAmount.ToString();
      amount.enabled = true;
   }

   public void ClearSlot() {
      item = null;
      icon.sprite = null;
      icon.enabled = false;
      amount.text = "";
      amount.enabled = false;
   }

   public void OnBeginDrag(PointerEventData eventData)
   {
      if (item != null) {
         originalParent = transform.parent;
         originalIndex = transform.GetSiblingIndex();
         clone = Instantiate(gameObject);
         clone.transform.SetParent(originalParent);
         clone.transform.SetSiblingIndex(originalIndex);
         transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, true);

         icon.raycastTarget = false;

         RectTransform imgTrans = icon.GetComponent<RectTransform>();
         left = imgTrans.offsetMin.x;
         bottom = imgTrans.offsetMin.y;
         right = imgTrans.offsetMax.x;
         top = imgTrans.offsetMax.x;

         icon.transform.position = transform.position;
         icon.enabled = true;
         amount.enabled = false;
         dragging = true;

         EventSystem.current.SetSelectedGameObject(null);
      }
   }

   public void OnDrag(PointerEventData eventData)
   {
      if (dragging) {
         icon.transform.position += (Vector3)eventData.delta;
      }
   }

   public void OnDrop(PointerEventData data) {
   }

   public void OnEndDrag(PointerEventData eventData)
   {
      if (dragging) {
         GameObject hoveredItem = eventData.pointerCurrentRaycast.gameObject;
         Transform slotHolder = hoveredItem.transform.parent.transform.parent;

         if (hoveredItem == null) {
            Inventory.instance.Drop(originalIndex);
         } else {
            int slotNr = hoveredItem.transform.parent.transform.GetSiblingIndex();
            if (hoveredItem.transform.parent.transform.parent.name != "DragParent" || hoveredItem.name == "Background") {
               if (slotHolder.tag == "Storage") {
                  // Move item from inventory to storage
                  Inventory.instance.AddToStorage(hoveredItem.transform.parent.transform.parent.gameObject, item, int.Parse(amount.text), slotNr, originalIndex);
               } else if (Inventory.instance.items[slotNr] == null) {
                  // Move item to empty field
                  Inventory.instance.Move(originalIndex, slotNr);
               } else if (hoveredItem.name == "Trash") {
                  // Destroy item
                  Inventory.instance.Destroy(originalIndex);
               } else {
                  // Swap items
                  Inventory.instance.Swap(originalIndex, hoveredItem.transform.parent.transform.parent.transform.GetSiblingIndex());
               } 
            }
         }

         Destroy(clone);
         transform.SetParent(originalParent, true);
         transform.SetSiblingIndex(originalIndex);

         icon.GetComponent<RectTransform>().offsetMin = new Vector2 (left, bottom);
         icon.GetComponent<RectTransform>().offsetMax = new Vector2 (right, top);
         dragging = false;
         amount.enabled = true;
         icon.raycastTarget = true;
      }
   }
}
