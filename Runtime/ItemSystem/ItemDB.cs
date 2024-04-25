using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "NewItemDatabase", menuName = "Item System/New Item Database", order = 2)]
    public class ItemDB : ScriptableObject
    {
        public ItemDataSO[] items;
    }
}
