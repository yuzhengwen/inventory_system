using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem
{
    public class UI_InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private InventorySlot slot;

        private GameObject uiInventoryItem;
        private Image icon;
        private TextMeshProUGUI stackSizeDisplay;
        private TextMeshProUGUI labelDisplay;

        private Image slotImage;
        private Color defaultColor;

        [SerializeField] private MouseDragItem draggable;

        private void Awake()
        {
            uiInventoryItem = transform.GetChild(0).gameObject;
            ClearUISlot();

            slotImage = GetComponent<Image>();
            defaultColor = slotImage.color;

            icon = uiInventoryItem.transform.Find("Icon").GetComponent<Image>();
            stackSizeDisplay = uiInventoryItem.transform.Find("StackSize").GetComponent<TextMeshProUGUI>();
            labelDisplay = uiInventoryItem.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        }
        /// <summary>
        /// Assigns the inventory slot to be tracked by this UI slot
        /// </summary>
        /// <param name="slot"></param>
        public void AssignSlot(InventorySlot slot)
        {
            this.slot = slot;
            SubscribeEvents();
        }
        // will not run on first enable since slot is not set
        private void OnEnable()
        {
            if (slot != null)
            {
                UpdateItem(slot);
                SubscribeEvents();
            }
        }
        private void OnDisable()
        {
            if (slot != null)
                UnsubscribeEvents();
        }
        private void SubscribeEvents()
        {
            slot.OnStackChanged += UpdateStackSize;
            slot.OnItemChanged += UpdateItem;
            slot.OnSlotCleared += ClearUISlot;
        }
        private void UnsubscribeEvents()
        {
            slot.OnStackChanged -= UpdateStackSize;
            slot.OnItemChanged -= UpdateItem;
            slot.OnSlotCleared -= ClearUISlot;
        }
        // automatically called when inventory slot being tracked becomes empty
        private void ClearUISlot()
        {
            uiInventoryItem.SetActive(false);
        }

        // automatically called when inventory slot being tracked changes item
        private void UpdateItem(InventorySlot slot)
        {
            if (!slot.IsOccupied())
            {
                ClearUISlot();
                return;
            }

            icon.sprite = slot.itemData.sprite;
            stackSizeDisplay.text = slot.stackSize.ToString();
            labelDisplay.text = slot.itemData.displayName;

            if (!uiInventoryItem.activeSelf) uiInventoryItem.SetActive(true);
        }
        // automatically called when inventory slot being tracked changes stack size
        private void UpdateStackSize(int stackSize, int change)
        {
            if (stackSize == 0)
            {
                ClearUISlot();
                return;
            }
            stackSizeDisplay.text = stackSize.ToString();
        }

        /// <summary>
        /// Returns the inventory slot being tracked
        /// </summary>
        /// <returns></returns>
        public InventorySlot GetItem()
        {
            return slot;
        }

        #region Mouse Hover effect
        public void OnPointerEnter(PointerEventData eventData)
        {
            slotImage.color = new Color(255, 255, 255, 0.8f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            slotImage.color = defaultColor;
        }
        #endregion

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                slot.UseItem();
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!slot.IsOccupied()) return;
            draggable.gameObject.SetActive(true);
            draggable.SetItem(this);
            ClearUISlot();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggable.gameObject.activeSelf) return;
            draggable.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggable.gameObject.activeSelf) return;
            draggable.gameObject.SetActive(false);

            var newSlot = CheckForValidSlot();
            if (draggable.from == newSlot || newSlot == null)
            {
                UpdateItem(draggable.from.GetItem());
                return;
            }

            // currently because InventorySlot is a class, we are passing by reference, so we can directly modify the slot
            InventorySlot item1 = draggable.from.GetItem(); // item being dragged
            InventorySlot item2 = newSlot.GetItem(); // item being dragged onto

            // if item is of the same type, stack them if possible
            if (item2.IsOccupied() && item1 == item2)
            {
                if (item2.stackSize + item1.stackSize <= item2.itemData.maxStackSize)
                {
                    item2.AddToStack(item1.stackSize);
                    item1.ClearSlot();
                }
                else
                {
                    int amountToMove = item2.itemData.maxStackSize - item2.stackSize;
                    item2.AddToStack(amountToMove);
                    item1.RemoveFromStack(amountToMove);
                }
            }
            else
                item1.Swap(item2);
        }
        private UI_InventorySlot CheckForValidSlot()
        {
            RaycastHit2D[] hits;
            hits = Physics2D.RaycastAll(draggable.transform.position, transform.forward, 100.0F);

            foreach (RaycastHit2D hit in hits)
            {
                UI_InventorySlot newSlot = hit.collider.gameObject.GetComponent<UI_InventorySlot>();
                if (newSlot != null && newSlot != draggable.from)
                    return newSlot;
            }
            return null;
        }
    }
}
