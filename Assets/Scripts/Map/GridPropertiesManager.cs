using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>, ISavable
{
    private Transform cropParentTransform;
    private Tilemap groundDecoration1;
    private Tilemap groundDecoration2;
    private bool isFirstTimeSceneLoaded = true;
    private Grid grid;
    private Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;

    [SerializeField] private SO_CropDetailsList sO_CropDetailsList = null;
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;
    [SerializeField] private Tile[] dugGround = null;
    [SerializeField] private Tile[] wateredGround = null;

    private string _iSavableUniqueID;
    public string ISavableUniqueID { get => _iSavableUniqueID; set => _iSavableUniqueID = value; }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get => _gameObjectSave; set => _gameObjectSave = value; }

    protected override void Awake()
    {
        base.Awake();
        ISavableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }
    private void OnEnable()
    {
        ISavableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }
    private void OnDisable()
    {
        ISavableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }
    public void ISavableRegister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Add(this);
    }
    public void ISavableDeregister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Remove(this);
    }

    private void Start()
    {
        //初始化
        InitializeGridProperties();
    }

    private void ClearDisplayGroundDecorations()
    {
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }

    private void ClearDisplayGridPropertyDetails()
    {
        ClearDisplayGroundDecorations();
        ClearDisplayAllPlantedCrops();//清除所有已种植作物
    }

    private void ClearDisplayAllPlantedCrops()
    {
        Crop[] crops;
        crops = FindObjectsOfType<Crop>();
        foreach (Crop crop in crops)
        {
            Destroy(crop.gameObject);
        }
    }

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);//将附近的土壤连接起来
        }
    }

    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);//将附近的浇水土壤连接起来
        }
    }

    //连接附近土壤
    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //中心瓦片
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), dugTile0);

        GridPropertyDetails nearbyGridPropertyDetails;
        //根据周围四格的土壤情况选择合适的瓦片
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), dugTile1);
        }
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), dugTile2);
        }
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), dugTile4);
        }
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), dugTile3);
        }

    }

    //连接附近浇水土壤
    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        //中心瓦片
        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        GridPropertyDetails nearbyGridPropertyDetails;
        //根据周围四格的浇水土壤情况选择合适的瓦片
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }
        nearbyGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (nearbyGridPropertyDetails != null && nearbyGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }

    //根据周围四格的土壤情况选择合适的瓦片
    private Tile SetDugTile(int gridX, int gridY)
    {
        bool upDug = IsGridSquareDug(gridX, gridY + 1);
        bool downDug = IsGridSquareDug(gridX, gridY - 1);
        bool rightDug = IsGridSquareDug(gridX + 1, gridY);
        bool leftDug = IsGridSquareDug(gridX - 1, gridY);

        if (!upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        else if (!upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[1];
        }
        else if (!upDug && downDug && rightDug && leftDug)
        {
            return dugGround[2];
        }
        else if (!upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[3];
        }
        else if (!upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        else if (upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[5];
        }
        else if (upDug && downDug && rightDug && leftDug)
        {
            return dugGround[6];
        }
        else if (upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[7];
        }
        else if (upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        else if (upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[9];
        }
        else if (upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[10];
        }
        else if (upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[11];
        }
        else if (upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        else if (!upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[13];
        }
        else if (!upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[14];
        }
        else if (!upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[15];
        }
        return null;
    }

    //根据周围四格的浇水土壤情况选择合适的瓦片
    private Tile SetWateredTile(int gridX, int gridY)
    {
        bool upWatered = IsGridSquareWatered(gridX, gridY + 1);
        bool downWatered = IsGridSquareWatered(gridX, gridY - 1);
        bool rightWatered = IsGridSquareWatered(gridX + 1, gridY);
        bool leftWatered = IsGridSquareWatered(gridX - 1, gridY);

        if (!upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[0];
        }
        else if (!upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[1];
        }
        else if (!upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[2];
        }
        else if (!upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[3];
        }
        else if (!upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[4];
        }
        else if (upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[5];
        }
        else if (upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[6];
        }
        else if (upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[7];
        }
        else if (upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[8];
        }
        else if (upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[9];
        }
        else if (upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[10];
        }
        else if (upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[11];
        }
        else if (upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[12];
        }
        else if (!upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[13];
        }
        else if (!upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[14];
        }
        else if (!upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[15];
        }
        return null;
    }

    //检查指定网格是否被挖
    private bool IsGridSquareDug(int gridX, int gridY)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(gridX, gridY);
        return gridPropertyDetails != null && gridPropertyDetails.daysSinceDug > -1;
    }

    private bool IsGridSquareWatered(int gridX, int gridY)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(gridX, gridY);
        return gridPropertyDetails != null && gridPropertyDetails.daysSinceWatered > -1;
    }

    private void DisplayGridPropertyDetails()
    {
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDetailsDictionary)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;

            DisplayDugGround(gridPropertyDetails);
            DisplayWateredGround(gridPropertyDetails);
            DisplayPlantedCrops(gridPropertyDetails);
        }
    }

    public void DisplayPlantedCrops(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.seedItemCode > -1)
        {
            CropDetails cropDetails = sO_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);
            if (cropDetails != null)
            {
                GameObject cropPrefab;

                int growthStages = cropDetails.growthDays.Length;

                int currentGrowthStage = 0;

                //更新生长阶段
                for (int i = growthStages - 1; i >= 0; i--)
                {
                    if (gridPropertyDetails.growthDays >= cropDetails.growthDays[i])
                    {
                        currentGrowthStage = i;
                        break;
                    }
                }
                cropPrefab = cropDetails.growthPrefabs[currentGrowthStage];
                Sprite growthSprite = cropDetails.growthSprites[currentGrowthStage];
                Vector3 worldPosition = groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y, worldPosition.z);

                //创建作物实例
                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);
                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;
                cropInstance.transform.SetParent(cropParentTransform);
                cropInstance.GetComponent<Crop>().cropGridPosition = new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }

        }
    }

    private void InitializeGridProperties()//在游戏开始时从SO_GridProperties中读取数据并初始化网格属性字典
    {

        foreach (SO_GridProperties so_gridProperties in so_gridPropertiesArray)
        {
            Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary = new Dictionary<string, GridPropertyDetails>();
            foreach (GridProperty gridProperty in so_gridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;
                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetailsDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();

                }
                switch (gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;
                }
                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y, gridPropertyDetails, gridPropertyDetailsDictionary);
            }
            SceneSave sceneSave = new SceneSave();
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;
            //将字典传入sceneSave
            if (so_gridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;
            }

            sceneSave.boolDictionary = new Dictionary<string, bool>();
            sceneSave.boolDictionary.Add("isFirstTimeSceneLoaded", true);

            GameObjectSave.sceneData.Add(so_gridProperties.sceneName.ToString(), sceneSave);
        }
    }

    //返回指定网格位置的作物
    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);//通过碰撞箱查找作物实体

        Crop crop = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            crop = colliders[i].gameObject.GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
            {
                break;
            }
            crop = colliders[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition == new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
            {
                break;
            }
        }
        return crop;
    }

    public CropDetails GetCropDetails(int seedItemCode)
    {
        return sO_CropDetailsList.GetCropDetails(seedItemCode);
    }

    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY, Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary)
    {
        string key = "x" + gridX + "y" + gridY;//根据坐标生成key

        GridPropertyDetails gridPropertyDetails;
        if (!gridPropertyDetailsDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }

    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDetailsDictionary);
    }

    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDetailsDictionary);
    }

    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails, Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary)
    {
        string key = "x" + gridX + "y" + gridY;//根据坐标生成key

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        gridPropertyDetailsDictionary[key] = gridPropertyDetails;
    }
    // private void AfterSceneLoaded()
    // {
    //     if (GameObject.FindGameObjectsWithTag(Tags.CropsParentTransform) != null)
    //     {
    //         cropParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
    //     }
    //     else
    //     {
    //         cropParentTransform = null;
    //     }

    //     //在场景加载后获取网格属性
    //     grid = GameObject.FindObjectOfType<Grid>();

    //     groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
    //     groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
    // }

    private void AfterSceneLoaded()
    {
        //找不到就赋值 null
        GameObject cropsParentObj = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform);
        if (cropsParentObj != null)
        {
            cropParentTransform = cropsParentObj.transform;
        }
        else
        {
            cropParentTransform = null;
        }

        grid = GameObject.FindObjectOfType<Grid>();

        GameObject groundDec1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1);
        if (groundDec1 != null)
        {
            groundDecoration1 = groundDec1.GetComponent<Tilemap>();
        }

        GameObject groundDec2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2);
        if (groundDec2 != null)
        {
            groundDecoration2 = groundDec2.GetComponent<Tilemap>();
        }
    }

    public void ISavableStoreScene(string sceneName)//保存场景
    {
        GameObjectSave.sceneData.Remove(sceneName);

        SceneSave sceneSave = new SceneSave();
        sceneSave.gridPropertyDetailsDictionary = gridPropertyDetailsDictionary;

        sceneSave.boolDictionary = new Dictionary<string, bool>();
        sceneSave.boolDictionary.Add("isFirstTimeSceneLoaded", this.isFirstTimeSceneLoaded);

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    public void ISavableRestoreScene(string sceneName)//加载场景
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDetailsDictionary = sceneSave.gridPropertyDetailsDictionary;
            }
            //判断是否首次加载
            if (sceneSave.boolDictionary != null && sceneSave.boolDictionary.TryGetValue("isFirstTimeSceneLoaded", out bool isFirstTimeSceneLoaded))
            {
                this.isFirstTimeSceneLoaded = isFirstTimeSceneLoaded;
            }
            if (this.isFirstTimeSceneLoaded)
            {
                EventHandler.CallInstantiateCropPrefabsEvent();
            }

            if (gridPropertyDetailsDictionary != null)
            {
                ClearDisplayGridPropertyDetails();
                DisplayGridPropertyDetails();
            }

            if (this.isFirstTimeSceneLoaded)
            {
                this.isFirstTimeSceneLoaded = false;
            }
        }
    }

    //处理每过一天游戏场景中的变化
    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        ClearDisplayGridPropertyDetails();

        //循环每个scene，重置浇水地块
        foreach (SO_GridProperties sO_GridProperties in so_gridPropertiesArray)
        {
            if (GameObjectSave.sceneData.TryGetValue(sO_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item = sceneSave.gridPropertyDetailsDictionary.ElementAt(i);
                        GridPropertyDetails gridPropertyDetails = item.Value;

                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays += 1;
                        }

                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);
                    }
                }
            }
        }

        DisplayGridPropertyDetails();
    }
}
