using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class InventorySlotOperationsTest 
{
    ItemDataSO coin;
    [OneTimeSetUp]
    public void Setup()
    {
        coin = ScriptableObject.CreateInstance<ItemDataSO>();
        coin.maxStackSize = 10;
    }
    [Test]
    public void SetItemTest()
    {
        InventorySlot slot = new(null);
        slot.SetItem(coin, 5);

        Assert.AreEqual(slot.stackSize, 5);
        Assert.AreEqual(slot.itemData, coin);
    }

    [Test]
    public void AddItemTest()
    {
        InventorySlot slot = new(null);
        slot.SetItem(coin, 1);
        slot.AddToStack(4);

        Assert.AreEqual(slot.stackSize, 5);
        Assert.AreEqual(slot.itemData, coin);
    }

    [Test]
    public void AddItemWithOverflowTest()
    {
        InventorySlot slot = new(null);
        slot.SetItem(coin, 5);

        int overflow = slot.AddToStack(10);
        Assert.AreEqual(overflow, 5);
        Assert.AreEqual(slot.IsMaxStack(), true);
        Assert.AreEqual(slot.itemData, coin);
    }
    [Test]
    public void RemoveItemTest()
    {
        InventorySlot slot = new(null);
        slot.SetItem(coin, 5);
        slot.RemoveFromStack(4);

        Assert.AreEqual(slot.stackSize, 1);
        Assert.AreEqual(slot.itemData, coin);
    }

    [Test]
    public void RemoveItemWithOverflowTest()
    {
        InventorySlot slot = new(null);
        slot.SetItem(coin, 5);

        int extra = slot.RemoveFromStack(10);
        Assert.AreEqual(extra, 5);
        Assert.AreEqual(slot.stackSize, 0);
        Assert.AreEqual(slot.itemData, null);
    }
}
