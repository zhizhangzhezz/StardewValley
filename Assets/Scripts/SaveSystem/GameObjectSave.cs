using System.Collections.Generic;
[System.Serializable]

public class GameObjectSave
{
    //key:sceneName
    public Dictionary<string, SceneSave> sceneData;
    public GameObjectSave()
    {
        sceneData = new Dictionary<string, SceneSave>();
    }
    public GameObjectSave(Dictionary<string, SceneSave> sceneData)
    {
        this.sceneData = sceneData;
    }
}
