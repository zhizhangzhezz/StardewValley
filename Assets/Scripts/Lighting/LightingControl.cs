using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class LightingControl : MonoBehaviour
{
    [SerializeField] private LightingSchedule lightingSchedule;
    [SerializeField] private bool isLightFlicker = false;
    [SerializeField] [Range(0f, 1f)] private float lightFlickerIntensity;

    private Light2D light2D;
    private Dictionary<string, float> lightingBrightnessDictionary = new Dictionary<string, float>();
    private float currentLightIntensity;
    private float lightFlickerTimer = 0f;
    private float lightFlickerTimeMin = 0f;
    private float lightFlickerTimeMax = 2f;
    private Coroutine fadeInLightRoutine;

    private void Awake()
    {
        light2D = GetComponentInChildren<Light2D>();

        if (light2D == null)
        {
            enabled = false;
        }

        foreach (LightingBrightness lightingBrightness in lightingSchedule.lightingBrightnessArray)
        {
            string key = lightingBrightness.season.ToString() + lightingBrightness.hour.ToString();
            lightingBrightnessDictionary.Add(key, lightingBrightness.lightIntensity);
        }
    }

    private void OnEnable()
    {
        EventHandler.AdvanceGameHourEvent += EventHandler_AdvanceGameHourEvent;
        EventHandler.AfterSceneLoadEvent += EventHandler_AfterSceneLoadEvent;
    }

    private void OnDisable()
    {
        EventHandler.AdvanceGameHourEvent -= EventHandler_AdvanceGameHourEvent;
        EventHandler.AfterSceneLoadEvent -= EventHandler_AfterSceneLoadEvent;
    }

    private void EventHandler_AdvanceGameHourEvent(int year, Season season, int day, string gameDayOfWeek, int hour, int minute, int second)
    {
        SetLightingIntensity(season, hour, true);
    }

    private void EventHandler_AfterSceneLoadEvent()
    {
        SetLightingAfterSceneLoaded();
    }

    private void Update()
    {
        if (isLightFlicker)
        {
            lightFlickerTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (lightFlickerTimer <= 0f && isLightFlicker)
        {
            LightFlicker();
        }
        else
        {
            light2D.intensity = currentLightIntensity;
        }
    }

    private void LightFlicker()
    {
        light2D.intensity = Random.Range(currentLightIntensity, currentLightIntensity + (currentLightIntensity * lightFlickerIntensity));
        lightFlickerTimer = Random.Range(lightFlickerTimeMin, lightFlickerTimeMax);
    }
    private void SetLightingAfterSceneLoaded()
    {
        Season season = TimeManager.Instance.GetGameSeason();
        int hour = TimeManager.Instance.GetGameTime().Hours;

        SetLightingIntensity(season, hour, false);
    }

    private void SetLightingIntensity(Season season, int hour, bool fadein)
    {
        //循环查找对应时间点对应光照强度
        int i = 0;
        while (i <= 23)
        {
            string key = season.ToString() + (hour).ToString();

            if (lightingBrightnessDictionary.TryGetValue(key, out float lightIntensity))
            {
                if (fadein)
                {
                    if (fadeInLightRoutine != null) StopCoroutine(fadeInLightRoutine);
                    fadeInLightRoutine = StartCoroutine(FadeInLightRoutine(lightIntensity));
                }
                else
                {
                    currentLightIntensity = lightIntensity;
                }
                break;
            }

            i++;
            hour--;
            if (hour < 0)
            {
                hour = 23;
            }
        }
    }

    private IEnumerator FadeInLightRoutine(float lightIntensity)
    {
        float fadeDuration = 5f;

        float fadeSpeed = Mathf.Abs(currentLightIntensity - lightIntensity) / fadeDuration;

        while (!Mathf.Approximately(currentLightIntensity, lightIntensity))
        {
            currentLightIntensity = Mathf.MoveTowards(currentLightIntensity, lightIntensity, fadeSpeed * Time.deltaTime);
            yield return null;

        }
        currentLightIntensity = lightIntensity;
    }
}
