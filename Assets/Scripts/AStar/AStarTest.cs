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
    [SerializeField] private NPCPath nPCPath = null;
    [SerializeField] private bool moveNPC = false;
    [SerializeField] private Vector2Int finishPos;
    [SerializeField] private AnimationClip idleDownAnimationClip = null;
    [SerializeField] private AnimationClip eventAnimationClip = null;
    [SerializeField] private SceneName sceneName = SceneName.Scene1_Farm;

    private NPCMovement npcMovement;

    private void Start()
    {
        npcMovement = nPCPath.GetComponent<NPCMovement>();
        npcMovement.npcFacingDirectionAtDestination = Direction.down;
        npcMovement.npcTargetAnimationClip = idleDownAnimationClip;
    }

    private void Update()
    {
        if (moveNPC)
        {
            moveNPC = false;

            NPCScheduleEvent npcScheduleEvent = new NPCScheduleEvent(0, 0, 0, 0, Weather.none, Season.none, sceneName, new GridCoordinate(finishPos.x, finishPos.y), eventAnimationClip);
            //Debug.Log($"【NPC Debug】点击 MoveNPC → 目标场景: {npcScheduleEvent.toSceneName} 目标坐标: {npcScheduleEvent.toGridCoordinate}");
            nPCPath.BuildPath(npcScheduleEvent);
        }
    }


}
