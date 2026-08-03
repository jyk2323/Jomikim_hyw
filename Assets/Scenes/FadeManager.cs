using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public float fadeDuration = 0.5f;
    private Image fadeImage;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        gameObject.AddComponent<CanvasScaler>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false; // ← 이거 추가! 클릭 막지 않음!

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void LoadScene(string sceneName, Vector2 spawnPoint)
    {
        StartCoroutine(FadeRoutine(sceneName, spawnPoint));
    }

    IEnumerator FadeRoutine(string sceneName, Vector2 spawnPoint)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }

        PlayerHealth.instance.transform.position = spawnPoint;
        SceneManager.LoadScene(sceneName);

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - timer / fadeDuration);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
    }
}