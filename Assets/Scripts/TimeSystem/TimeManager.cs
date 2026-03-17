using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>
{
    private int gameYear = 1;
    private Season gameSeason = Season.Spring;
    private int gameDay = 1;
    private string gameDayOfWeek = "Mon";
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;

    private bool gameClockPaused = false;
    private float gameTick = 0f;


    private void Start()
    {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        }
    }
    private void GameTick()
    {
        gameTick += Time.deltaTime;
        if (gameTick >= Settings.secondsPerGameScond)
        {
            gameTick -= Settings.secondsPerGameScond;
            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;
        if (gameSecond >= 60)
        {
            gameSecond = 0;
            UpdateGameMinute();
        }
    }
    private void UpdateGameMinute()
    {
        gameMinute++;
        if (gameMinute >= 60)
        {
            gameMinute = 0;
            UpdateGameHour();
        }
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        //Debug.Log("Game Year: " + gameYear + " Game Season: " + gameSeason + " Game Day: " + gameDay + " Game Day of Week: " + gameDayOfWeek + " Game Hour: " + gameHour + " Game Minute: " + gameMinute + " Game Second: " + gameSecond);
    }
    private void UpdateGameHour()
    {
        gameHour++;
        if (gameHour >= 24)
        {
            gameHour = 0;
            UpdateGameDay();
        }
        EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    private void UpdateGameDay()
    {
        gameDay++;
        if (gameDay > 30)
        {
            gameDay = 1;
            UpdateGameSeason();
        }
        gameDayOfWeek = GetDayOfWeek();
        EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    private void UpdateGameSeason()
    {
        int gs = (int)gameSeason;
        gs++;
        gameSeason = (Season)gs;
        if (gs > 3)
        {
            gs = 0;
            gameYear++;

            if (gameYear > 9999)
            {
                gameYear = 1;
            }
            gameSeason = (Season)gs;
            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
        }
        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private string GetDayOfWeek()
    {
        int totalDays = ((int)gameSeason) * 30 + gameDay - 1;
        int dayOfWeek = totalDays % 7;
        switch (dayOfWeek)
        {
            case 0:
                return "Mon";
            case 1:
                return "Tue";
            case 2:
                return "Wed";
            case 3:
                return "Thu";
            case 4:
                return "Fri";
            case 5:
                return "Sat";
            case 6:
                return "Sun";
            default:
                return "";
        }
    }


    //测试加速时间
    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }
    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }
}
