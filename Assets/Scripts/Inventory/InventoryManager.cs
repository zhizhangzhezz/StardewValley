using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>, ISavable
{
    private UIInventoryBar inventoryBar;
    private Dictionary<int, ItemDetails> itemDetailsDictionary;//物品详情字典

    private int[] selectedInventoryItem;//选中的物品

    public List<InventoryItem>[] inventoryLists;//玩家库存列表

    [HideInInspector] public int[] inventoryListCapacityIntArray;

    [SerializeField] private SO_itemList itemList = null;

    //保存物品相关
    private string _iSavableUniqueID;
    public string ISavableUniqueID { get { return _iSavableUniqueID; } set { _iSavableUniqueID = value; } }
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

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
        ISavableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISavableRegister();
    }

    private void OnDisable()
    {
        ISavableDeregister();
    }

    private void Start()
    {
        inventoryBar = FindObjectOfType<UIInventoryBar>();
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

        // 用空占位符初始化每个列表到其容量（如果 capacity 为 0 则不填充）
        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            int cap = inventoryListCapacityIntArray[i];
            for (int j = 0; j < cap; j++)
            {
                InventoryItem empty = new InventoryItem();
                empty.itemCode = -1; // -1 表示空槽位
                empty.itemQuantity = 0;
                inventoryLists[i].Add(empty);
            }
        }
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
            // 优先寻找第一个空槽位填充
            int emptySlot = FindFirstEmptySlot(inventoryList);
            if (emptySlot != -1)
            {
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem.itemCode = itemCode;
                inventoryItem.itemQuantity = 1;
                inventoryList[emptySlot] = inventoryItem;
            }
            else
            {
                // 没有空槽则追加（兼容原实现或扩容）
                AddItemAtPosition(inventoryList, itemCode);
            }
        }
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            // 优先寻找第一个空槽位填充
            int emptySlot = FindFirstEmptySlot(inventoryList);
            if (emptySlot != -1)
            {
                InventoryItem inventoryItem = new InventoryItem();
                inventoryItem.itemCode = itemCode;
                inventoryItem.itemQuantity = 1;
                inventoryList[emptySlot] = inventoryItem;
            }
            else
            {
                // 没有空槽则追加（兼容原实现或扩容）
                AddItemAtPosition(inventoryList, itemCode);
            }
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

    // 返回第一个空槽位索引（itemCode == -1），找不到返回 -1
    private int FindFirstEmptySlot(List<InventoryItem> inventoryList)
    {
        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == -1)
            {
                return i;
            }
        }
        return -1;
    }

    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    //返回物品详情类
    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);
        if (itemCode != -1)
        {
            return GetItemDetails(itemCode);
        }
        else
        {
            return null;
        }
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

        // 如果被选中的物品已被完全消耗（在库存中找不到），清除选中状态并确保玩家不再携带该物品
        if (GetSelectedInventoryItem(inventoryLocation) == itemCode && FindItemInInventory(inventoryLocation, itemCode) == -1)
        {
            ClearSelectedInventoryItem(inventoryLocation);
            if (Player.Instance != null)
            {
                Player.Instance.ClearCarriedItem();
            }
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
            // 不移除元素，保留空占位以避免后续物品前移
            InventoryItem empty = new InventoryItem();
            empty.itemCode = -1;
            empty.itemQuantity = 0;
            inventoryList[itemPosition] = empty;
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

    public void ISavableRegister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Add(this);
    }
    public void ISavableDeregister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Remove(this);
    }

    public GameObjectSave ISavableSave()
    {
        SceneSave sceneSave = new SceneSave();
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);
        //加入库存信息
        sceneSave.listInventoryItems = inventoryLists;
        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityIntArray", inventoryListCapacityIntArray);

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISavableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISavableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.listInventoryItems != null)
                {
                    inventoryLists = sceneSave.listInventoryItems;
                    //恢复玩家库存
                    for (int i = 0; i < (int)InventoryLocation.count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
                    }
                    //清空举起的物品和选择的物品
                    Player.Instance.ClearCarriedItem();

                    inventoryBar.ClearHighlightOnInventorySlots();
                }

                if (sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityIntArray", out int[] capacityArray))
                {
                    inventoryListCapacityIntArray = capacityArray;
                }
            }
        }
    }

    public void ISavableStoreScene(string sceneName)
    {
        //不需要实现
    }
    public void ISavableRestoreScene(string sceneName)
    {
        //不需要实现
    }
}
