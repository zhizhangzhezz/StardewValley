using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(NPCMovement))]
[RequireComponent(typeof(GenerateGUID))]
public class NPC : MonoBehaviour, ISavable
{
    private string _iSavableUniqueID;
    public string ISavableUniqueID { get { return _iSavableUniqueID; } set { _iSavableUniqueID = value; } }
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }
    private NPCMovement npcMovement;

    private void OnEnable()
    {
        ISavableRegister();
    }
    private void OnDisable()
    {
        ISavableDeregister();
    }

    private void Awake()
    {
        ISavableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start()
    {
        npcMovement = GetComponent<NPCMovement>();
    }
    public void ISavableRegister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Add(this);
    }
    public void ISavableDeregister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Remove(this);
    }

    public void ISavableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISavableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                if (sceneSave.vector3Dictionary != null && sceneSave.stringDictionary != null)
                {
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGridPosition", out Vector3Serializable npcTargetGridPos))
                    {
                        npcMovement.npcTargetGridPosition = new Vector3Int((int)npcTargetGridPos.x, (int)npcTargetGridPos.y, (int)npcTargetGridPos.z);
                        npcMovement.npcCurrentGridPosition = npcMovement.npcTargetGridPosition;
                    }

                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGWorldPosition", out Vector3Serializable npcTargetWorldPos))
                    {
                        npcMovement.npcTargetWorldPosition = new Vector3Int((int)npcTargetWorldPos.x, (int)npcTargetWorldPos.y, (int)npcTargetWorldPos.z);
                        transform.position = npcMovement.npcTargetWorldPosition;
                    }

                    if (sceneSave.stringDictionary.TryGetValue("npcTargetScene", out string npcTargetScene))
                    {
                        if (Enum.TryParse<SceneName>(npcTargetScene, out SceneName sceneName))
                        {
                            npcMovement.npcTargetScene = sceneName;
                            npcMovement.npcCurrentScene = npcMovement.npcTargetScene;
                        }
                    }

                    npcMovement.CancelNPCMovement();
                }
            }
        }
    }

    //保存npc目标点
    public GameObjectSave ISavableSave()
    {
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        SceneSave sceneSave = new SceneSave();

        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();
        sceneSave.stringDictionary = new Dictionary<string, string>();

        sceneSave.vector3Dictionary.Add("npcTargetGridPosition", new Vector3Serializable(npcMovement.npcTargetGridPosition.x, npcMovement.npcTargetGridPosition.y, npcMovement.npcTargetGridPosition.z));
        sceneSave.vector3Dictionary.Add("npcTargetWorldPosition", new Vector3Serializable(npcMovement.npcTargetWorldPosition.x, npcMovement.npcTargetWorldPosition.y, npcMovement.npcTargetWorldPosition.z));
        sceneSave.stringDictionary.Add("npcTargetScene", npcMovement.npcTargetScene.ToString());

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISavableRestoreScene(string name)
    {

    }
    public void ISavableStoreScene(string name)
    {

    }
}
