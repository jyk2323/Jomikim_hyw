using UnityEngine;
using System.Collections;

public class ErrorSequence : MonoBehaviour
{
    public static ErrorSequence instance;

    public GameObject[] errorPanels; // 에러창들 Inspector에서 연결!
    public float errorInterval = 0.5f; // 에러창 뜨는 간격

    public string nextSceneName; // 보스전 씬 이름

    void Awake()
    {
        instance = this;
    }

    public void StartErrors()
    {
        StartCoroutine(ShowErrors());
    }

    IEnumerator ShowErrors()
    {
        // 에러창 순서대로 띄우기
        foreach (GameObject panel in errorPanels)
        {
            panel.SetActive(true);
            yield return new WaitForSeconds(errorInterval);
        }

        // 에러창 다 뜨면 잠깐 대기
        yield return new WaitForSeconds(1f);

        // Fade Out 후 씬 이동!
        FadeManager.instance.LoadScene(nextSceneName, Vector2.zero);
    }
}