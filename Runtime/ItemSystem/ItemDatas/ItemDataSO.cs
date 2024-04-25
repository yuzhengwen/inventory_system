using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item System/Item Data", order = 1)]
public class ItemDataSO : ScriptableObject
{
    public string displayName;
    public Sprite sprite;
    public int id;
    public int maxStackSize;

    public ItemType itemType;

    public bool useable;
    public int amtConsumedOnUse;
}

public enum ItemType
{
    General, Currency, Equipment
}