using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite transparentCursorSprite = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private GridCursor gridCursor = null;

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }

    private float _itemUseRadius = 0;
    public float ItemUseRadius { get => _itemUseRadius; set => _itemUseRadius = value; }

    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    private bool _cursorIsEnabled = false;
    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }


    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (CursorIsEnabled)
        {
            DisplayCursor();
        }
    }
    private void DisplayCursor()
    {
        Vector3 cursorWorldPosition = GetWorldPositionForCursor();
        SetCursorValidity(cursorWorldPosition, Player.Instance.GetPlayerCenterPosition());
        cursorRectTransform.position = GetRectTransformPositionForCursor();
    }

    //返回光标在世界中的位置
    public Vector3 GetWorldPositionForCursor()
    {
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    //返回玩家在网格中的位置
    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }

    private void SetCursorValidity(Vector3 cursorPosition, Vector3 playerPosition)
    {
        //光标设为有效
        SetCursorToValid();


        if (cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f) ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f) ||
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f) ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f))
        {
            //在工具使用范围之外，光标设为无效
            SetCursorToInvalid();
            return;
        }

        if (Mathf.Abs(cursorPosition.x - playerPosition.x) > ItemUseRadius || Mathf.Abs(cursorPosition.y - playerPosition.y) > ItemUseRadius)
        {
            //在工具使用范围之外，光标设为无效
            SetCursorToInvalid();
            return;
        }

        //获取手中的物品
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);
        if (itemDetails == null)
        {
            //光标设为无效
            SetCursorToInvalid();
            return;
        }

        switch (itemDetails.itemType)
        {
            case ItemType.BreakingTool:
            case ItemType.ChoppingTool:
            case ItemType.HoeingTool:
            case ItemType.WateringTool:
            case ItemType.ReapingTool:
            case ItemType.CollectingTool:
                if (!SetCursorValidForTool(cursorPosition, playerPosition, itemDetails))
                {
                    //光标设为无效
                    SetCursorToInvalid();
                    return;
                }
                break;

            case ItemType.none:
                break;

            case ItemType.count:
                break;

            default:
                break;
        }

    }

    private bool SetCursorValidForTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        switch (itemDetails.itemType)
        {
            case ItemType.ReapingTool:
                return SetCursorValidReapingTool(cursorPosition, playerPosition, itemDetails);
            default:
                return false;
        }
    }

    private bool SetCursorValidReapingTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        List<Item> itemList = new List<Item>();

        if (HelperMethods.GetComponentAtCursorLocation<Item>(out itemList, cursorPosition))
        {
            if (itemList.Count == 0)
            {
                return false;
            }
            foreach (Item item in itemList)
            {
                if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void SetCursorToValid()
    {
        CursorPositionIsValid = true;
        cursorImage.sprite = greenCursorSprite;
        //不显示此光标时显示grid光标
        gridCursor.DisableCursor();
    }

    private void SetCursorToInvalid()
    {
        CursorPositionIsValid = false;
        cursorImage.sprite = transparentCursorSprite;

        gridCursor.EnableCursor();
    }
    public void DisableCursor()
    {
        CursorIsEnabled = false;
        cursorImage.color = Color.clear;

    }
    public void EnableCursor()
    {
        CursorIsEnabled = true;
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
    }

    //获取光标在UI中的位置
    public Vector2 GetRectTransformPositionForCursor()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        return RectTransformUtility.PixelAdjustPoint(screenPosition, cursorRectTransform, canvas);
    }
}
