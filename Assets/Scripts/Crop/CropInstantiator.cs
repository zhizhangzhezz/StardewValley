using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropInstantiator : MonoBehaviour
{
    private Grid grid;
    [SerializeField] private int daysSinceDug = -1;
    [SerializeField] private int daysSinceWatered = -1;
    [ItemCodeDescription]
    [SerializeField] private int seedItemCode = 0;
    [SerializeField] private int growthDays = 0;

    private void OnEnable()
    {
        EventHandler.InstantiateCropPrefabsEvent += InstantiateCropPrefabs;
    }

    private void OnDisable()
    {
        EventHandler.InstantiateCropPrefabsEvent -= InstantiateCropPrefabs;
    }

    private void InstantiateCropPrefabs()
    {
        // 实例化农作物预制体
        grid = GameObject.FindObjectOfType<Grid>();

        Vector3Int cropGridPosition = grid.WorldToCell(transform.position);
        //设置农作物网格属性
        SetCropGridProperties(cropGridPosition);

        Destroy(gameObject);
    }

    private void SetCropGridProperties(Vector3Int cropGridPosition)
    {
        if (seedItemCode > 0)
        {
            GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y);

            if (gridPropertyDetails != null)
            {
                gridPropertyDetails = new GridPropertyDetails();
            }

            gridPropertyDetails.daysSinceDug = daysSinceDug;
            gridPropertyDetails.daysSinceWatered = daysSinceWatered;
            gridPropertyDetails.seedItemCode = seedItemCode;
            gridPropertyDetails.growthDays = growthDays;

            GridPropertiesManager.Instance.SetGridPropertyDetails(cropGridPosition.x, cropGridPosition.y, gridPropertyDetails);
        }
    }

}
