using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AStar))]

public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    [SerializeField] private SO_SceneRouteList so_SceneRouteList = null;
    private Dictionary<string, SceneRoute> sceneRouteDictionary;

    [HideInInspector] public NPC[] npcArray;

    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();
        aStar = GetComponent<AStar>();
        npcArray = FindObjectsOfType<NPC>();

        //初始化场景路径字典
        sceneRouteDictionary = new Dictionary<string, SceneRoute>();
        if (so_SceneRouteList.sceneRoutes.Count > 0)
        {
            foreach (SceneRoute sceneRoute in so_SceneRouteList.sceneRoutes)
            {
                if (sceneRouteDictionary.ContainsKey(sceneRoute.fromSceneName.ToString() + sceneRoute.toSceneName.ToString()))
                {
                    Debug.Log("场景路径字典已包含从" + sceneRoute.fromSceneName.ToString() + "到" + sceneRoute.toSceneName.ToString() + "的路径，跳过添加");
                    continue;
                }
                //以"fromSceneName+toSceneName"作为键，场景路径作为值添加到字典中，方便后续查询
                sceneRouteDictionary.Add(sceneRoute.fromSceneName.ToString() + sceneRoute.toSceneName.ToString(), sceneRoute);
            }

        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }
    private void AfterSceneLoad()
    {
        SetNPCsActiveStatus();
    }

    private void SetNPCsActiveStatus()
    {
        foreach (NPC npc in npcArray)
        {
            NPCMovement npcMovement = npc.GetComponent<NPCMovement>();
            //npc在玩家当前场景时才可见
            if (npcMovement.npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
            {
                npcMovement.SetNPCActiveInScene();
            }
            else
            {
                npcMovement.SetNPCInactiveInScene();
            }
        }
    }

    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int targetGridPosition, Stack<NPCMovementStep> nPCMovementSteps)
    {
        return aStar.BuildPath(sceneName, startGridPosition, targetGridPosition, nPCMovementSteps);
    }

    //获取字典中已存在的路径
    public SceneRoute GetSceneRoute(string fromSceneName, string toSceneName)
    {
        SceneRoute sceneRoute;
        if (sceneRouteDictionary.TryGetValue(fromSceneName + toSceneName, out sceneRoute))
        {
            return sceneRoute;
        }
        else
        {
            return null;
        }
    }
}
