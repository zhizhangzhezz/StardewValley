using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CropDetails
{
    [ItemCodeDescription]
    public int seedItemCode;//对应种子的物品代码
    public int[] growthDays;//每阶段的生长天数
    public int totalGrowthDays;//总天数
    public GameObject[] growthPrefabs;
    public Sprite[] growthSprites;
    public Season[] seasons;
    public Sprite harvestedSprite;//被收割后的sprite

    [ItemCodeDescription]
    public int harvestedTransformItemCode;//若收割后还有其他阶段
    public bool hideCropBeforeHarvestedAnimation;
    public bool disableCropColliderBeforeHarvestedAnimation;
    public bool isHarvestedAnimation;
    public bool isHarvestActionEffect = false;
    public bool spawnCropProducedAtPlayerPosition;
    public HarvestActionEffect harvestActionEffect;

    [ItemCodeDescription]
    public int[] harvestToolItemCode;
    public int[] requiredHarvestActions;

    [ItemCodeDescription]
    public int[] cropProducedItemCode;
    public int[] cropProducedMinQuantity;
    public int[] cropProducedMaxQuantity;
    public int daysToGrow;

    public bool CanUseToolToHarvestCrop(int toolItemCode)
    {
        if (RequiredHarvestActionsForTool(toolItemCode) == -1)
        {//不能用工具收获
            return false;
        }
        return true;
    }

    public int RequiredHarvestActionsForTool(int toolItemCode)
    {
        for (int i = 0; i < harvestToolItemCode.Length; i++)
        {
            if (harvestToolItemCode[i] == toolItemCode)
            {
                return requiredHarvestActions[i];
            }
        }
        return -1;
    }
}
