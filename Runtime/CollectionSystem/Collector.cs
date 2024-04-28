using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem
{
    [RequireComponent(typeof(Collider2D))]
    public class Collector : MonoBehaviour
    {
        [Header("List of Inventory & Accepted Item Types")]
        [SerializeField] private InventoryGroup[] inventoryGroups;
        [Header("List of Inventory Views and Model to bind")]
        [SerializeField] UI_InventoryGroup[] uiGroups;
        private bool fullInventoryOpened = false;

        [Header("Keybinds")]
        [SerializeField] private InputAction toggleInventoryAction;
        [SerializeField] private InputAction closeAction;

        public event Action<ItemDataSO> OnItemCollected;
        private void Start()
        {
            foreach (var uiGroup in uiGroups)
                uiGroup.uiInventory.AssignInventory(uiGroup.inventoryToAssign);
            CloseFullInventory();
            if (toggleInventoryAction != null)
            {
                toggleInventoryAction.performed += (ctx) => ToggleFullInventory();
                toggleInventoryAction.Enable();
            }
            if (closeAction != null)
            {
                closeAction = new("Close Inventory", InputActionType.Button, "<Keyboard>/escape");
                closeAction.performed += (ctx) => CloseFullInventory();
                closeAction.Enable();
            }
        }
        private void OnDisable()
        {
            toggleInventoryAction.Disable();
            closeAction.Disable();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ICollectible collectible = collision.GetComponent<ICollectible>();
            collectible?.Collect(this);
        }
        public void AddItemsToInventory(ItemDataSO itemData, int amt = 1)
        {
            foreach (var invGroup in inventoryGroups)
            {
                if (invGroup.acceptAllTypes)
                {
                    invGroup.inventory.AddItem(itemData, amt);
                    continue;
                }
                foreach (var type in invGroup.acceptedTypes)
                {
                    if (itemData.itemType == type)
                    {
                        invGroup.inventory.AddItem(itemData, amt);
                        break;
                    }
                }
            }
            OnItemCollected?.Invoke(itemData);
        }
        #region Opening Inventory
        public void OpenFullInventory()
        {
            if (fullInventoryOpened) return;
            foreach (var uiGroup in uiGroups) uiGroup.Open();
            fullInventoryOpened = true;
        }
        public void CloseFullInventory()
        {
            foreach (var uiGroup in uiGroups) uiGroup.Close();
            fullInventoryOpened = false;
        }
        public void ToggleFullInventory()
        {
            if (fullInventoryOpened)
                CloseFullInventory();
            else
                OpenFullInventory();
        }
        #endregion
    }
    [System.Serializable]
    public class UI_InventoryGroup
    {
        public UI_Inventory uiInventory;
        public Inventory inventoryToAssign;
        public bool canToggle = true;
        public bool visibleOnOpen = true;
        public bool visibleOnClose = false;

        public void Open()
        {
            if (canToggle)
                uiInventory.gameObject.SetActive(visibleOnOpen);
        }
        public void Close()
        {
            if (canToggle)
                uiInventory.gameObject.SetActive(visibleOnClose);
        }
    }
    [System.Serializable]
    public class InventoryGroup
    {
        public Inventory inventory;
        public ItemType[] acceptedTypes;
        public bool acceptAllTypes;
    }
}
