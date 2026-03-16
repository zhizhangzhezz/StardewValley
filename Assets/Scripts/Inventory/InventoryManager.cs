using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int, ItemDetails> itemDetailsDictionary;//物品详情字典

    private int[] selectedInventoryItem;//选中的物品

    public List<InventoryItem>[] inventoryLists;//玩家库存列表

    [HideInInspector] public int[] inventoryListCapacityIntArray;

    [SerializeField] private SO_itemList itemList = null;

    protected override void Awake()
    {
        base.Awake();
        CreateItemDetailsDictionary();
        CreateInventoryLists();
        selectedInventoryItem = new int[(int)InventoryLocation.count];//初始化选中的物品数组，-1表示没有选中物品
        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            selectedInventoryItem[i] = -1;
        }
    }

    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    //创建玩家库存列表
    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);
        Destroy(gameObjectToDelete);//拾取后删除gameobject
    }
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }

    //在指定位置添加物品(当前未有该物品)
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

        // DebugPrintInventoryList(inventoryList);
    }

    //在指定位置添加物品(当前已有该物品)
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();
        int quantity = inventoryList[itemPosition].itemQuantity;
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = quantity + 1;
        inventoryList[itemPosition] = inventoryItem;

        // Debug.ClearDeveloperConsole();
        // DebugPrintInventoryList(inventoryList);
    }

    //根据itemCode返回物品详情
    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }

    //返回物品描述
    public string GetItemTypeDescription(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.HoeingTool:
                return Settings.HoeingTool;
            case ItemType.ChoppingTool:
                return Settings.ChoppingTool;
            case ItemType.BreakingTool:
                return Settings.BreakingTool;
            case ItemType.ReapingTool:
                return Settings.ReapingTool;
            case ItemType.WateringTool:
                return Settings.WateringTool;
            case ItemType.CollectingTool:
                return Settings.CollectingTool;
            default:
                return itemType.ToString();
        }
    }

    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    //在指定位置移除物品
    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();
        int quantity = inventoryList[itemPosition].itemQuantity - 1;
        if (quantity > 0)
        {
            inventoryItem.itemCode = itemCode;
            inventoryItem.itemQuantity = quantity;
            inventoryList[itemPosition] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(itemPosition);
        }

    }

    //交换物品
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;
            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }
    }

    private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    {
        Debug.Log("Inventory List:");
        foreach (InventoryItem item in inventoryList)
        {
            Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(item.itemCode).itemDescription + " Quantity:" + item.itemQuantity);
        }
    }

    //设置选中的物品
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    //清除选中的物品
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }
}
