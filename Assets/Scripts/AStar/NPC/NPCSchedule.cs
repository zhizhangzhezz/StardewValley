using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCPath))]
public class NPCSchedule : MonoBehaviour
{
    [SerializeField] private SO_NPCScheduleEventList so_NPCScheduleEventList = null;
    private SortedSet<NPCScheduleEvent> npcScheduleEventSet;
    private NPCPath npcPath;

    private void Awake()
    {
        npcScheduleEventSet = new SortedSet<NPCScheduleEvent>(new NPCScheduleEventSort());

        foreach (NPCScheduleEvent scheduleEvent in so_NPCScheduleEventList.npcScheduleEventList)
        {
            npcScheduleEventSet.Add(scheduleEvent);
        }

        npcPath = GetComponent<NPCPath>();
    }
    private void OnEnable()
    {
        EventHandler.AdvanceGameMinuteEvent += GameTimeSystem_AdvanceMinute;
    }
    private void OnDisable()
    {
        EventHandler.AdvanceGameMinuteEvent -= GameTimeSystem_AdvanceMinute;
    }

    private void GameTimeSystem_AdvanceMinute(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        int time = (gameHour * 100) + gameMinute;

        NPCScheduleEvent matchingNPCScheduleEvent = null;
        //查找匹配的时间点
        foreach (NPCScheduleEvent nPCScheduleEvent in npcScheduleEventSet)
        {
            if (nPCScheduleEvent.Time == time)
            {
                if (nPCScheduleEvent.day != 0 && nPCScheduleEvent.day != gameDay)
                {
                    continue;
                }
                if (nPCScheduleEvent.season != Season.none && nPCScheduleEvent.season != gameSeason)
                {
                    continue;
                }
                if (nPCScheduleEvent.weather != Weather.none && nPCScheduleEvent.weather != GameManager.Instance.currentWeather)
                {
                    continue;
                }

                matchingNPCScheduleEvent = nPCScheduleEvent;
                break;
            }
            else if (nPCScheduleEvent.Time > time)
            {
                break;
            }
        }

        if (matchingNPCScheduleEvent != null)
        {
            npcPath.BuildPath(matchingNPCScheduleEvent);
        }

    }
}
