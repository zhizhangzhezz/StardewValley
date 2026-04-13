using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{

    public Weather currentWeather;
    protected override void Awake()
    {
        base.Awake();

        Screen.SetResolution(1920, 1000, FullScreenMode.FullScreenWindow, 0);

        currentWeather = Weather.dry;
    }
}
