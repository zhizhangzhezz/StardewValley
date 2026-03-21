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

            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);

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
