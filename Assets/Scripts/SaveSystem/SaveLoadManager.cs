using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public List<ISavable> iSavableObjectList;//可保存对象列表
    protected override void Awake()
    {
        base.Awake();
        iSavableObjectList = new List<ISavable>();
    }
    public void StoreCurrentScene()//存储当前场景
    {
        foreach (ISavable savableObject in iSavableObjectList)
        {
            savableObject.ISavableStoreScene(SceneManager.GetActiveScene().name);
        }
    }
    public void RestoreCurrentScene()//恢复当前场景
    {
        foreach (ISavable savableObject in iSavableObjectList)
        {
            savableObject.ISavableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }

}
