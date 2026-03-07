using UnityEngine;

[System.Serializable]

public class ItemDetails 
{
    public int itemCode;
    public ItemType itemType;
    public string itemDescription;
    public Sprite itemSprite;
    public string itemLongDescription;
    public short itemUseGridRadius;//物品使用网格半径
    public float itemUseRadius;//物品攻击范围
    public bool isStaringItem;
    public bool canBePickedup;
    public bool canBeDropped;
    public bool canBeEaten;
    public bool canBeCarried;
}
