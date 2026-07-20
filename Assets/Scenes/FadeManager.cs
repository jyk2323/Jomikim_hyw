using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;

    public float fadeDuration = 0.8f; // 인스펙터에서 조절 가능!

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

        // 검은 화면 이미지 자동 생성
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);

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
        // 페이드 아웃 (화면이 검어짐)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, timer / fadeDuration);
            yield return null;
        }

        // 씬 전환
        PlayerHealth.instance.transform.position = spawnPoint;
        SceneManager.LoadScene(sceneName);

        // 페이드 인 (화면이 밝아짐)
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