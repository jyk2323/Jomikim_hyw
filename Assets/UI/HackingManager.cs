using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HackingManager : MonoBehaviour
{
    public static HackingManager Instance { get; private set; }

    [Header("진행도 스프라이트 (loding1~4 순서대로)")]
    [SerializeField] private Sprite[] progressSprites;   // 4장

    [Header("미니게임 씬 이름 (진행 순서대로)")]
    [SerializeField] private string[] minigameScenes;    // 3개

    [Header("미니게임 표시 위치")]
    [Tooltip("비워두면 메인 카메라 중앙에 띄웁니다.")]
    [SerializeField] private Transform minigameAnchor;
    [SerializeField] private UnityEngine.Vector2 minigameOffset = UnityEngine.Vector2.zero;

    [Header("이벤트")]
    public UnityEvent onHackComplete;

    private Image progressImage;
    private int stage;
    private bool running;
    private bool waitingResult;   // 결과를 기다리는 중일 때만 true (중복 호출 방지)
    private string currentScene;

    private void Awake() => Instance = this;

    public void StartHacking(Image targetImage)
    {
        if (running) return;
        running = true;
        stage = 0;
        progressImage = targetImage;
        UpdateProgress();
        StartCoroutine(LoadStage());
    }

    private IEnumerator LoadStage()
    {
        currentScene = minigameScenes[stage];
        yield return SceneManager.LoadSceneAsync(currentScene, LoadSceneMode.Additive);

        Scene scene = SceneManager.GetSceneByName(currentScene);   // ← scene은 여기서 선언
        DisableDuplicates(scene);
        PlaceMinigame(scene);

        waitingResult = true;
        Debug.Log($"[Hacking] 단계 {stage + 1} 시작: {currentScene}");
    }

    // 미니게임 쪽에서 끝날 때 호출
    public void OnMinigameFinished(bool success)
    {
        if (!running || !waitingResult)
        {
            Debug.Log($"[Hacking] 중복/무효 호출 무시 (success={success})");
            return;
        }
        waitingResult = false;
        Debug.Log($"[Hacking] 단계 {stage + 1} 결과 수신: success={success}");
        StartCoroutine(FinishRoutine(success));
    }

    private IEnumerator FinishRoutine(bool success)
    {
        var op = SceneManager.UnloadSceneAsync(currentScene);
        if (op != null) yield return op;

        if (success)
        {
            stage++;
            UpdateProgress();

            if (stage >= minigameScenes.Length)
            {
                running = false;
                Debug.Log("[Hacking] 해킹 완료");
                onHackComplete?.Invoke();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        // 실패하면 같은 단계를 다시 로드해요 (재도전)
        yield return LoadStage();
    }

    private void UpdateProgress()
    {
        if (progressImage != null && stage < progressSprites.Length)
            progressImage.sprite = progressSprites[stage];
    }

    // ───── EventSystem / AudioListener 중복 끄기 ─────

    private void DisableDuplicates(Scene scene)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            foreach (var es in go.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true))
                es.enabled = false;
            foreach (var al in go.GetComponentsInChildren<AudioListener>(true))
                al.enabled = false;
        }
    }

    // ───── 미니게임 위치 맞추기 ─────

    private void PlaceMinigame(Scene scene)
    {
        Vector3 target;
        if (minigameAnchor != null) target = minigameAnchor.position;
        else if (Camera.main != null) target = Camera.main.transform.position;
        else return;
        target += (Vector3)minigameOffset;

        var movable = new List<GameObject>();
        Vector3 sum = Vector3.zero;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (ShouldSkip(go)) continue;
            movable.Add(go);
            sum += go.transform.position;
        }
        if (movable.Count == 0) return;

        Vector3 center = sum / movable.Count;
        Vector3 delta = new Vector3(target.x - center.x, target.y - center.y, 0f);

        foreach (var go in movable)
            go.transform.position += delta;   // 배치 간격은 유지한 채 통째로 이동
    }

    private bool ShouldSkip(GameObject go)
    {
        if (go.GetComponent<Camera>() != null) return true;
        if (go.GetComponent<AudioListener>() != null) return true;
        if (go.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) return true;

        // 화면 고정 Canvas(Overlay/Camera)는 위치 이동이 의미 없으니 제외
        var canvas = go.GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.WorldSpace) return true;

        return false;
    }
}