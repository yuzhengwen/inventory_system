using System;
using UnityEngine;

namespace InventorySystem
{
    [Serializable]
    public class InventorySlot
    {
        public ItemDataSO itemData; // stores static shared data
        public int stackSize;

        #region Events
        /// <summary>
        /// Triggered when only stack size changes (but not the item)
        /// Passes the new stack size & change amount
        /// </summary>
        public event Action<int, int> OnStackChanged;
        /// <summary>
        /// Triggered when item is changed in the slot
        /// Passes the slot after the change
        /// </summary>
        public event Action<InventorySlot> OnItemChanged;
        /// <summary>
        /// Triggered when slot is cleared
        /// </summary>
        public event Action OnSlotCleared;
        #endregion
        private Inventory inv;
        public InventorySlot(Inventory inv)
        {
            this.inv = inv;
        }
        public void ClearSlot()
        {
            OnSlotCleared?.Invoke();
            itemData = null;
            stackSize = 0;
        }
        /// <summary>
        /// Wrapper method for SetItem(ItemDataSO, int, BaseInventoryItem)
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public InventorySlot SetItem(InventorySlot slot)
        {
            if (slot == null)
            {
                ClearSlot(); return this;
            }
            return SetItem(slot.itemData, slot.stackSize);
        }
        public InventorySlot SetItem(ItemDataSO itemData, int stackSize)
        {
            if (itemData == null || stackSize <= 0)
                ClearSlot();
            this.itemData = itemData;
            this.stackSize = stackSize;
            OnItemChanged?.Invoke(this);
            return this;
        }
        /// <summary>
        /// Increases stack size
        /// NOTE: Please call this through Inventory class to ensure events are triggered
        /// </summary>
        /// <param name="amount">Amount to add to stack</param>
        /// <returns>Amount of items unable to be added (overflow)</returns>
        public int AddToStack(int amount)
        {
            if (IsMaxStack())
            {
                return amount;
            }
            else
            {
                stackSize += amount;
                int overflow = 0;
                if (stackSize > itemData.maxStackSize)
                {
                    overflow = stackSize - itemData.maxStackSize;
                    stackSize = itemData.maxStackSize;
                }
                OnStackChanged?.Invoke(stackSize, amount);
                return overflow;
            }
        }
        /// <summary>
        /// Decreases the stack size 
        /// NOTE: Please call this through Inventory class to ensure events are triggered
        /// </summary>
        /// <param name="amount">Amount to remove from stack</param>
        /// <returns>Amount of items that still need to be removed</returns>
        public int RemoveFromStack(int amount)
        {
            stackSize -= amount;
            int excess = 0;
            if (stackSize <= 0)
            {
                excess = -stackSize;
                ClearSlot();
            }
            OnStackChanged?.Invoke(stackSize, -amount);
            return excess;
        }
        /// <summary>
        /// Creates a full copy of the slot (not a reference)
        /// </summary>
        /// <returns></returns>
        public InventorySlot Copy()
        {
            return new InventorySlot(inv) { itemData = itemData, stackSize = stackSize };
        }
        /// <summary>
        /// Swaps the data between the two slots
        /// If slot passed is null, the slot is cleared
        /// </summary>
        public void Swap(InventorySlot slot)
        {
            // due to pass by reference, we need to create a full copy of the slot being swapped to
            // If we use temp = slot, then changes to slot will also change temp
            InventorySlot temp = slot.Copy();
            slot.SetItem(this);
            SetItem(temp);
        }

        public void UseItem()
        {
            if (!IsOccupied() || !itemData.useable) return;
            if (stackSize < itemData.amtConsumedOnUse)
            {
                Debug.Log("<color=red>Not enough</color> in stack to use!");
                return;
            }
            inv.OnItemUsed?.Invoke(this, itemData);
            RemoveFromStack(itemData.amtConsumedOnUse);
        }
        public bool IsMaxStack() => itemData ? stackSize == itemData.maxStackSize : false;
        public bool IsOccupied() => itemData != null && stackSize > 0;
        public bool IsSameItem(InventorySlot slot) => itemData == slot.itemData;
    }
}
