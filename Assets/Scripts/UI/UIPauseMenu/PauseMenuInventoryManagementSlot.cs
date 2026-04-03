using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuInventoryManagementSlot : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image inventoryManagementSlotImage;
    public GameObject greyedOutImageGO;
    public TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private PauseMenuInventoryManagement inventoryManagement = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;

    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;

    //private Vector3 startingPosition;
    public GameObject draggedItem;
    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemQuantity != 0)
        {
            //初始化物品为被拖动物品
            draggedItem = Instantiate(inventoryManagement.inventoryManagementDraggedItemPrefab, inventoryManagement.transform);

            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            Destroy(draggedItem);
            //结束拖拽
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null)
            {
                //获取目标槽位
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>().slotNumber;
                //交换物品
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                inventoryManagement.DestroyInventoryTextBoxGameObject();
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemQuantity != 0)
        {
            //初始化物品信息框
            inventoryManagement.inventoryTextBoxGameObject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryManagement.inventoryTextBoxGameObject.transform.SetParent(parentCanvas.transform, false);

            UIInventoryTextBox inventoryTextBox = inventoryManagement.inventoryTextBoxGameObject.GetComponent<UIInventoryTextBox>();

            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            inventoryTextBox.SetTextBoxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            if (slotNumber > 23)
            {
                inventoryManagement.inventoryTextBoxGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                inventoryManagement.inventoryTextBoxGameObject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryManagement.inventoryTextBoxGameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryManagement.inventoryTextBoxGameObject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryManagement.DestroyInventoryTextBoxGameObject();
    }
}
