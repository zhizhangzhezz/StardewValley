using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>, ISavable
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

    private string _iSavableUniqueID;
    public string ISavableUniqueID { get { return _iSavableUniqueID; } set { _iSavableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    protected override void Awake()
    {
        base.Awake();
        ISavableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISavableRegister();

        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoadFadeIn;
    }

    private void OnDisable()
    {
        ISavableDeregister();

        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnloadFadeOut;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoadFadeIn;
    }

    private void BeforeSceneUnloadFadeOut()
    {
        //暂停游戏时间
        gameClockPaused = true;
    }

    private void AfterSceneLoadFadeIn()
    {
        //恢复游戏时间
        gameClockPaused = false;
    }

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

    public void ISavableRegister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Add(this);
    }
    public void ISavableDeregister()
    {
        SaveLoadManager.Instance.iSavableObjectList.Remove(this);
    }

    public GameObjectSave ISavableSave()
    {
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        SceneSave sceneSave = new SceneSave();
        sceneSave.intDictionary = new Dictionary<string, int>();
        sceneSave.stringDictionary = new Dictionary<string, string>();
        //存入时间信息
        sceneSave.intDictionary.Add("gameYear", gameYear);
        sceneSave.intDictionary.Add("gameDay", gameDay);
        sceneSave.intDictionary.Add("gameHour", gameHour);
        sceneSave.intDictionary.Add("gameMinute", gameMinute);
        sceneSave.intDictionary.Add("gameSecond", gameSecond);

        sceneSave.stringDictionary.Add("gameDayOfWeek", gameDayOfWeek);
        sceneSave.stringDictionary.Add("gameSeason", gameSeason.ToString());

        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISavableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISavableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                //加载时间信息
                if (sceneSave.intDictionary.TryGetValue("gameYear", out int gy))
                {
                    gameYear = gy;
                }
                if (sceneSave.intDictionary.TryGetValue("gameDay", out int gd))
                {
                    gameDay = gd;
                }
                if (sceneSave.stringDictionary.TryGetValue("gameDayOfWeek", out string gdw))
                {
                    gameDayOfWeek = gdw;
                }
                if (sceneSave.intDictionary.TryGetValue("gameHour", out int gh))
                {
                    gameHour = gh;
                }
                if (sceneSave.intDictionary.TryGetValue("gameMinute", out int gm))
                {
                    gameMinute = gm;
                }
                if (sceneSave.intDictionary.TryGetValue("gameSecond", out int gs))
                {
                    gameSecond = gs;
                }
                if (sceneSave.stringDictionary.TryGetValue("gameSeason", out string gameSeasonString))
                {
                    if (Enum.TryParse(gameSeasonString, out Season season))
                    {
                        gameSeason = season;
                    }
                }

                gameTick = 0f;

                EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }
        }
    }
    public void ISavableStoreScene(string sceneName)
    {

    }
    public void ISavableRestoreScene(string sceneName)
    {

    }
}
