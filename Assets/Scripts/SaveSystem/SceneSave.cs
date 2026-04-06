using System.Collections.Generic;
[System.Serializable]
public class SceneSave
{
    public Dictionary<string, bool> boolDictionary;
    public Dictionary<string, string> stringDictionary;
    public Dictionary<string, Vector3Serializable> vector3Dictionary;

    //场景中的物品列表
    public List<SceneItem> listSceneItem;
    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;

    //保存玩家库存
    public Dictionary<string, int[]> intArrayDictionary;
    public List<InventoryItem>[] listInventoryItems;

    //保存游戏时间
    public Dictionary<string, int> intDictionary;

}
