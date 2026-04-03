using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuInventoryManagement : MonoBehaviour
{
    [SerializeField] private PauseMenuInventoryManagementSlot[] inventoryManagementSlots = null;
    public GameObject inventoryManagementDraggedItemPrefab;
    [SerializeField] private Sprite transparent16x16 = null;
    [HideInInspector] public GameObject inventoryTextBoxGameObject;

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;

        if (InventoryManager.Instance != null)
        {
            PopulatePlayerInventory(InventoryLocation.player, InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player]);
        }
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;

        DestroyInventoryTextBoxGameObject();
    }
    //销毁描述框
    public void DestroyInventoryTextBoxGameObject()
    {
        if (inventoryTextBoxGameObject != null)
        {
            Destroy(inventoryTextBoxGameObject);
        }
    }
    //销毁当前拖拽物品
    public void DestroyCurrentDraggedItem()
    {
        for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)
        {
            if (inventoryManagementSlots[i].draggedItem != null)
            {
                Destroy(inventoryManagementSlots[i].draggedItem);
            }
        }
    }

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            InitializeInventoryManagementSlots();

            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)
            {
                //获取物品详情
                inventoryManagementSlots[i].itemDetails = InventoryManager.Instance.GetItemDetails(inventoryList[i].itemCode);
                inventoryManagementSlots[i].itemQuantity = inventoryList[i].itemQuantity;
                //更新图标与文本
                if (inventoryManagementSlots[i].itemDetails != null)
                {
                    inventoryManagementSlots[i].inventoryManagementSlotImage.sprite = inventoryManagementSlots[i].itemDetails.itemSprite;
                    inventoryManagementSlots[i].textMeshProUGUI.text = inventoryManagementSlots[i].itemQuantity.ToString();
                }
            }
        }
    }

    private void InitializeInventoryManagementSlots()
    {
        //先清空
        for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            inventoryManagementSlots[i].greyedOutImageGO.SetActive(false);
            inventoryManagementSlots[i].itemDetails = null;
            inventoryManagementSlots[i].itemQuantity = 0;
            inventoryManagementSlots[i].inventoryManagementSlotImage.sprite = transparent16x16;
            inventoryManagementSlots[i].textMeshProUGUI.text = "";
        }
        //灰化未解锁的格子
        for (int i = InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.player]; i < Settings.playerMaximumInventoryCapacity; i++)
        {
            inventoryManagementSlots[i].greyedOutImageGO.SetActive(true);
        }
    }
}
