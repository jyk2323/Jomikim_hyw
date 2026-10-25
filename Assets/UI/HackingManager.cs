using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HackingManager : MonoBehaviour
{
    public static HackingManager Instance { get; private set; }

    [Header("진행도 UI")]
    [SerializeField] private GameObject progressPanel;   // ProgressImage 오브젝트
    [SerializeField] private Image progressImage;
    [SerializeField] private Sprite[] progressSprites;   // 4장 (loding1~4 순서대로)

    [Header("미니게임 씬 이름 (진행 순서대로)")]
    [SerializeField] private string[] minigameScenes;    // 3개

    [Header("이벤트")]
    public UnityEvent onHackComplete;

    private int stage;
    private bool running;
    private string currentScene;

    private void Awake() => Instance = this;

    // 하드웨어와 상호작용할 때 호출
    public void StartHacking()
    {
        if (running) return;
        running = true;
        stage = 0;
        progressPanel.SetActive(true);
        UpdateProgress();
        StartCoroutine(LoadStage());
    }

    private IEnumerator LoadStage()
    {
        currentScene = minigameScenes[stage];
        yield return SceneManager.LoadSceneAsync(currentScene, LoadSceneMode.Additive);
    }

    // 미니게임 쪽에서 끝날 때 호출
    public void OnMinigameFinished(bool success)
    {
        StartCoroutine(FinishRoutine(success));
    }

    private IEnumerator FinishRoutine(bool success)
    {
        yield return SceneManager.UnloadSceneAsync(currentScene);

        if (success)
        {
            stage++;
            UpdateProgress();

            if (stage >= minigameScenes.Length)
            {
                running = false;
                onHackComplete?.Invoke();   // 문 열기, 다음 연출 등
                yield break;
            }
            yield return new WaitForSeconds(0.5f);  // 진행도 바뀐 걸 보여줄 시간
        }
        // 실패하면 같은 단계를 다시 로드해요 (재도전)
        yield return LoadStage();
    }

    private void UpdateProgress()
    {
        progressImage.sprite = progressSprites[stage];
    }
}