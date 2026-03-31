using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite redCursorSprite = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private SO_CropDetailsList sO_CropDetailsList = null;

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }

    private int _itemUserGridRadius = 0;
    public int ItemUserGridRadius { get => _itemUserGridRadius; set => _itemUserGridRadius = value; }

    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    private bool _cursorIsEnabled = false;
    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }
    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

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
    private Vector3Int DisplayCursor()
    {
        if (grid != null)
        {
            Vector3Int gridPosition = GetGridPositionForCursor();
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            SetCursorValidity(gridPosition, playerGridPosition);

            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);//光标跟随鼠标显示

            return gridPosition;
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    //返回光标在网格中的位置
    public Vector3Int GetGridPositionForCursor()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        return grid.WorldToCell(worldPosition);
    }

    //返回光标在世界中的位置
    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }

    //返回玩家在网格中的位置
    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);
    }

    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        //光标设为有效
        SetCursorToValid();

        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUserGridRadius || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUserGridRadius)
        {
            //光标设为无效
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
        //获取光标所在网格的属性
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);
        if (gridPropertyDetails != null)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails))
                    {
                        //光标设为无效
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))
                    {
                        //光标设为无效
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.HoeingTool:
                case ItemType.BreakingTool:
                case ItemType.ChoppingTool:
                case ItemType.ReapingTool:
                case ItemType.CollectingTool:
                case ItemType.WateringTool:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemDetails))
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
        else
        {
            //光标设为无效
            SetCursorToInvalid();
            return;
        }

    }

    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }
    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;
    }
    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        switch (itemDetails.itemType)
        {
            case ItemType.HoeingTool:
                if (gridPropertyDetails.isDiggable && gridPropertyDetails.daysSinceDug == -1)
                {
                    //获取光标位置
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f, 0f);

                    //获取光标位置的场景物体
                    List<Item> itemList = new List<Item>();

                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);

                    //若有可收割的物品，则不能耕地
                    bool foundReapable = false;
                    foreach (Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)
                        {
                            foundReapable = true;
                            break;
                        }
                    }
                    if (foundReapable)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }

                }
                else
                {
                    return false;
                }

            case ItemType.WateringTool:
                if (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1)//耕过且未浇过水的地块才能浇水
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case ItemType.ChoppingTool:
            case ItemType.BreakingTool:
            case ItemType.CollectingTool:
                //已有作物种植
                if (gridPropertyDetails.seedItemCode != -1)
                {
                    CropDetails cropDetails = sO_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);
                    if (cropDetails != null)
                    {
                        if (gridPropertyDetails.growthDays >= cropDetails.growthDays[cropDetails.growthDays.Length - 1])//已经成熟
                        {
                            if (cropDetails.CanUseToolToHarvestCrop(itemDetails.itemCode))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;

            default:
                return false;
        }
    }

    private void SetCursorToValid()
    {
        CursorPositionIsValid = true;
        cursorImage.sprite = greenCursorSprite;
    }

    private void SetCursorToInvalid()
    {
        CursorPositionIsValid = false;
        cursorImage.sprite = redCursorSprite;
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
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);
    }
}
