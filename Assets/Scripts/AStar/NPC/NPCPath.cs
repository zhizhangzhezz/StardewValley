using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(NPCMovement))]
public class NPCPath : MonoBehaviour
{
    public Stack<NPCMovementStep> nPCMovementSteps;

    private NPCMovement npcMovement;

    private void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();
        nPCMovementSteps = new Stack<NPCMovementStep>();
    }
    public void ClearPath()
    {
        nPCMovementSteps.Clear();
    }

    public void BuildPath(NPCScheduleEvent npcScheduleEvent)
    {
        ClearPath();

        //Debug.Log($"【NPC Debug】开始构建路径 → 当前场景: {npcMovement.npcCurrentScene} 目标场景: {npcScheduleEvent.toSceneName}");

        //同场景
        if (npcScheduleEvent.toSceneName == npcMovement.npcCurrentScene)
        {
            Vector2Int npcCurrentGridPosition = (Vector2Int)npcMovement.npcCurrentGridPosition;

            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            //Debug.Log($"【NPC Debug】同场景寻路 → 起点: {npcCurrentGridPosition} 终点: {npcTargetGridPosition}");
            //更新路径栈
            NPCManager.Instance.BuildPath(npcScheduleEvent.toSceneName, npcCurrentGridPosition, npcTargetGridPosition, nPCMovementSteps);

        }
        //跨场景
        else if (npcScheduleEvent.toSceneName != npcMovement.npcCurrentScene)
        {
            SceneRoute sceneRoute;
            //获取路径
            sceneRoute = NPCManager.Instance.GetSceneRoute(npcMovement.npcCurrentScene.ToString(), npcScheduleEvent.toSceneName.ToString());

            if (sceneRoute != null)
            {
                //Debug.Log($"【NPC Debug】跨场景路线找到 → 总段数: {sceneRoute.scenePathList.Count}");
                //反向遍历，检查是否为终点/起点
                for (int i = sceneRoute.scenePathList.Count - 1; i >= 0; --i)
                {
                    int toGridX, toGridY, fromGridX, fromGridY;

                    ScenePath scenePath = sceneRoute.scenePathList[i];

                    if (scenePath.toGridCell.x >= Settings.maxGridWidth || scenePath.toGridCell.y >= Settings.maxGridHeight)
                    {
                        toGridX = npcScheduleEvent.toGridCoordinate.x;
                        toGridY = npcScheduleEvent.toGridCoordinate.y;
                    }
                    else
                    {
                        toGridX = scenePath.toGridCell.x;
                        toGridY = scenePath.toGridCell.y;
                    }

                    if (scenePath.fromGridCell.x >= Settings.maxGridWidth || scenePath.fromGridCell.y >= Settings.maxGridHeight)
                    {
                        fromGridX = npcMovement.npcCurrentGridPosition.x;
                        fromGridY = npcMovement.npcCurrentGridPosition.y;
                    }
                    else
                    {
                        fromGridX = scenePath.fromGridCell.x;
                        fromGridY = scenePath.fromGridCell.y;
                    }

                    Vector2Int fromGridPosition = new Vector2Int(fromGridX, fromGridY);

                    Vector2Int toGridPosition = new Vector2Int(toGridX, toGridY);

                    //Debug.Log($"【NPC Debug】跨场景子路径 {i} → 场景: {scenePath.sceneName} 起点: {fromGridPosition} 终点: {toGridPosition}");

                    //设置路径
                    NPCManager.Instance.BuildPath(scenePath.sceneName, fromGridPosition, toGridPosition, nPCMovementSteps);
                }
            }
        }

        if (nPCMovementSteps.Count > 1)
        {
            //Debug.Log($"【NPC Debug】路径构建完成 → 总步数: {nPCMovementSteps.Count}");

            UpdateTimesOnPath();

            nPCMovementSteps.Pop();//更新路径栈

            npcMovement.SetScheduleEventDetails(npcScheduleEvent);
        }
    }

    public void UpdateTimesOnPath()
    {
        TimeSpan currentGameTime = TimeManager.Instance.GetGameTime();
        //Debug.Log($"【NPC Debug】开始给路径设置时间 → 当前游戏时间: {currentGameTime}");
        NPCMovementStep previousMovementStep = null;

        foreach (NPCMovementStep npcMovementStep in nPCMovementSteps)
        {
            if (previousMovementStep == null)
            {
                previousMovementStep = npcMovementStep;
            }

            npcMovementStep.hour = currentGameTime.Hours;
            npcMovementStep.minute = currentGameTime.Minutes;
            npcMovementStep.second = currentGameTime.Seconds;

            TimeSpan movementTimeStep;
            //对角移动
            if (MovementIsDiagonal(npcMovementStep, previousMovementStep))
            {
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / Settings.secondsPerGameScond / npcMovement.npcNormalSpeed));
            }
            else
            {
                movementTimeStep = new TimeSpan(0, 0, (int)(Settings.gridCellSize / Settings.secondsPerGameScond / npcMovement.npcNormalSpeed));
            }

            currentGameTime = currentGameTime.Add(movementTimeStep);
            previousMovementStep = npcMovementStep;
        }
    }

    private bool MovementIsDiagonal(NPCMovementStep currentStep, NPCMovementStep previousStep)
    {
        if (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x && currentStep.gridCoordinate.y != previousStep.gridCoordinate.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
