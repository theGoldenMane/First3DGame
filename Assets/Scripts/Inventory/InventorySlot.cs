using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour,
   IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
   public Image icon;
   private float left = 0f;
   private float right = 0f;
   private float top = 0f;
   private float bottom = 0f;

   Item item;

   private bool dragging = false;

   public void AddItem(Item newItem) {
      item = newItem;
      icon.sprite = item.icon;
      icon.enabled = true;
   }

   public void ClearSlot() {
      item = null;
      icon.sprite = null;
      icon.enabled = false;
   }

   public void OnBeginDrag(PointerEventData eventData)
   {
      if (item != null) {
         RectTransform imgTrans = icon.GetComponent<RectTransform>();
         left = imgTrans.offsetMin.x;
         bottom = imgTrans.offsetMin.y;
         right = imgTrans.offsetMax.x;
         top = imgTrans.offsetMax.x;

         icon.transform.position = transform.position;
         icon.enabled = true;
         dragging = true;
      }
   }

   public void OnDrag(PointerEventData eventData)
   {
      if (dragging) {
         icon.transform.position += (Vector3)eventData.delta;
      }
   }

   public void OnDrop(PointerEventData data) {}

   public void OnEndDrag(PointerEventData eventData)
   {
      Debug.Log(eventData.hovered.Count);
      List<GameObject> list = eventData.hovered;
      for (int i = 0; i < list.Count; i++) {
         Debug.Log(list[i]);
      }

      if (dragging) {
         string hoveredItem = eventData.hovered[eventData.hovered.Count - 1].name;

         if (hoveredItem == "Button") {
            // Move item to empty field
            Debug.Log("Move item to " + eventData.hovered[eventData.hovered.Count - 2].name);
         } else if (hoveredItem == "Icon") {
            // Swap items
            Debug.Log("Swap" + eventData.hovered[eventData.hovered.Count - 2].name);
         } else {
            //Drop Item
            Inventory.instance.Drop(item);
         }
         icon.GetComponent<RectTransform>().offsetMin = new Vector2 (left, bottom);
         icon.GetComponent<RectTransform>().offsetMax = new Vector2 (right, top);
         dragging = false;
      }
   }
}
