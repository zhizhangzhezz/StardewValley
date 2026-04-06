using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    public GameSave gameSave;//游戏存档
    public List<ISavable> iSavableObjectList;//可保存对象列表
    protected override void Awake()
    {
        base.Awake();
        iSavableObjectList = new List<ISavable>();
    }

    public void LoadDataFromFile()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/gamesave.dat"))
        {
            gameSave = new GameSave();

            FileStream fileStream = File.Open(Application.persistentDataPath + "/gamesave.dat", FileMode.Open);
            gameSave = (GameSave)binaryFormatter.Deserialize(fileStream);//从文件中加载游戏存档

            for (int i = iSavableObjectList.Count - 1; i > -1; i--)
            {
                if (gameSave.gameObjectData.ContainsKey(iSavableObjectList[i].ISavableUniqueID))
                {
                    iSavableObjectList[i].ISavableLoad(gameSave);
                }
                else
                {
                    Component component = (Component)iSavableObjectList[i];
                    Destroy(component.gameObject);
                }
            }
            fileStream.Close();

            // 如果从暂停菜单加载，确保关闭暂停菜单
            if (UIManager.Instance != null)
            {
                UIManager.Instance.DisableMenu();
            }
        }

    }

    public void SaveDataToFile()
    {
        gameSave = new GameSave();
        foreach (ISavable savableObject in iSavableObjectList)
        {
            gameSave.gameObjectData.Add(savableObject.ISavableUniqueID, savableObject.ISavableSave());
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(Application.persistentDataPath + "/gamesave.dat", FileMode.Create);
        binaryFormatter.Serialize(fileStream, gameSave);//将游戏存档保存到文件
        fileStream.Close();

        UIManager.Instance.DisableMenu();
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
