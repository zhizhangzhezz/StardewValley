using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
    private bool isFading;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup faderCanvasGroup = null;
    [SerializeField] private Image faderImage = null;
    public SceneName startingSceneName;

    private IEnumerator Fade(float targetAlpha)
    {
        isFading = true;

        faderCanvasGroup.blocksRaycasts = true;
        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - targetAlpha) / fadeDuration;

        while (Mathf.Abs(faderCanvasGroup.alpha - targetAlpha) > 0.01f)
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;//等待一帧
        }

        faderCanvasGroup.alpha = targetAlpha;
        isFading = false;
        faderCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeAndSwitchScenes(string sceneName, Vector3 spawnPosition)
    {
        //先淡出
        EventHandler.CallBeforeSceneUnloadFadeOutEvent();
        yield return StartCoroutine(Fade(1f));
        Player.Instance.gameObject.transform.position = spawnPosition;

        //再卸载当前场景    
        EventHandler.CallBeforeSceneUnloadEvent();
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        yield return StartCoroutine(LoadSceneAndSetActive(sceneName.ToString()));

        EventHandler.CallAfterSceneLoadEvent();
        yield return StartCoroutine(Fade(0f));
        EventHandler.CallAfterSceneLoadFadeInEvent();
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);//异步加载场景
        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);//获取最后加载的场景
        SceneManager.SetActiveScene(newScene);
    }

    private IEnumerator Start()
    {
        faderImage.color = Color.black;
        faderCanvasGroup.alpha = 1f;

        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName.ToString()));
        EventHandler.CallAfterSceneLoadEvent();
        StartCoroutine(Fade(0f));

    }

    public void FadeAndLoadScene(string sceneName, Vector3 spawnPosition)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScenes(sceneName, spawnPosition));
        }

    }
}
