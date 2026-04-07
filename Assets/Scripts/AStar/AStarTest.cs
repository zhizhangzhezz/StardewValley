using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(AStar))]

public class AStarTest : MonoBehaviour
{
    private AStar aStar;
    [SerializeField] private Vector2Int startPos;
    [SerializeField] private Vector2Int targetPos;
    [SerializeField] private Tilemap tilemapToDisplayPathOn = null;
    [SerializeField] private TileBase tileToUseToDisplayPath = null;
    [SerializeField] private bool displayStartAndFinish = false;
    [SerializeField] private bool displayPath = false;

    private Stack<NPCMovementStep> npcMovementSteps;

    private void Awake()
    {
        aStar = GetComponent<AStar>();
        npcMovementSteps = new Stack<NPCMovementStep>();
    }
    private void Start()
    {

    }
    private void Update()
    {
        // 必要的引用校验
        if (tilemapToDisplayPathOn == null || tileToUseToDisplayPath == null)
            return;

        // display start/finish 标记（startPos/targetPos 是格子坐标时直接使用）
        if (displayStartAndFinish)
        {
            tilemapToDisplayPathOn.SetTile(new Vector3Int(startPos.x, startPos.y, 0), tileToUseToDisplayPath);
            tilemapToDisplayPathOn.SetTile(new Vector3Int(targetPos.x, targetPos.y, 0), tileToUseToDisplayPath);
        }
        else
        {
            tilemapToDisplayPathOn.SetTile(new Vector3Int(startPos.x, startPos.y, 0), null);
            tilemapToDisplayPathOn.SetTile(new Vector3Int(targetPos.x, targetPos.y, 0), null);
        }

        if (displayPath)
        {
            Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, out SceneName sceneName);

            // 关键：清空上一次的结果，避免累积
            npcMovementSteps.Clear();

            // 构建路径到清空的栈
            aStar.BuildPath(sceneName, startPos, targetPos, npcMovementSteps);

            // 绘制当前路径
            foreach (NPCMovementStep npcMovementStep in npcMovementSteps)
            {
                tilemapToDisplayPathOn.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x, npcMovementStep.gridCoordinate.y, 0), tileToUseToDisplayPath);
            }
        }
        else
        {
            // 清理上次显示的 path
            if (npcMovementSteps.Count > 0)
            {
                foreach (NPCMovementStep npcMovementStep in npcMovementSteps)
                {
                    tilemapToDisplayPathOn.SetTile(new Vector3Int(npcMovementStep.gridCoordinate.x, npcMovementStep.gridCoordinate.y, 0), null);
                }
                npcMovementSteps.Clear();
            }
        }
    }
}
