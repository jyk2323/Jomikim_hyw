using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파란 원(ShrinkingCircle)을 15단계 스프라이트(큰 원 -> 작은 원)로 순서대로 교체하며
/// "계단식"으로 축소시키고, 검은 원(TargetCircle)과 크기가 맞아떨어지는 프레임 부근에서
/// 스페이스바를 누르면 성공, 아니면 실패 처리하는 스크립트.
/// 
/// 계층 구조 예시 (Canvas 하위):
///   CircleUnit (빈 오브젝트, RectTransform만 있음. Manager가 위치를 잡아줌)
///     ├─ TargetCircle (Image - 검은 원, 고정 크기)
///     └─ ShrinkingCircle (Image - 파란 원, 이 스크립트 부착)
/// </summary>
[RequireComponent(typeof(Image))]
public class ShrinkCircleController : MonoBehaviour
{
    public enum JudgeResult { Perfect, Good, Fail, Miss }

    [Header("프레임(스프라이트) 목록")]
    [Tooltip("반드시 '큰 원(0번) -> 작은 원(마지막 번)' 순서로 15개를 넣어주세요.")]
    public Sprite[] shrinkFrames;

    [Header("타이밍 설정")]
    [Tooltip("0번 프레임에서 마지막 프레임까지 도달하는 데 걸리는 총 시간(초)")]
    public float shrinkDuration = 1.5f;

    [Header("판정 기준 (프레임 '개수' 오차 기준)")]
    [Tooltip("검은 원과 크기가 일치한다고 볼 프레임 번호 (0부터 시작, 보통 마지막 프레임이나 그 직전)")]
    public int targetFrameIndex = 14;
    [Tooltip("targetFrameIndex와 정확히 일치할 때만 Perfect (0 = 오차 없음)")]
    public int perfectFrameRange = 0;
    [Tooltip("이 프레임 수 이내 오차면 Good (성공 처리)")]
    public int goodFrameRange = 1;
    [Tooltip("이 프레임 수를 넘으면 클릭해도 실패, 클릭이 없어도 이 지점을 지나면 자동 Miss")]
    public int failFrameRange = 3;

    [Header("입력 설정")]
    public KeyCode judgeKey = KeyCode.Space;

    private RectTransform selfRect;
    private Image selfImage;
    private float timer = 0f;
    private float frameDuration;
    private int currentFrameIndex = -1;
    private bool isJudged = false;
    private bool isRunning = false;

    /// <summary>Manager가 판정 결과를 받기 위한 콜백</summary>
    public System.Action<ShrinkCircleController, JudgeResult> OnJudged;

    void Awake()
    {
        selfRect = GetComponent<RectTransform>();
        selfImage = GetComponent<Image>();
    }

    /// <summary>Manager에서 스폰 직후 호출해서 라운드 시작</summary>
    public void StartRound()
    {
        if (shrinkFrames == null || shrinkFrames.Length < 2)
        {
            Debug.LogError("[ShrinkCircleController] shrinkFrames가 비어있거나 부족합니다. 15개의 프레임을 순서대로 등록해주세요.");
            return;
        }

        timer = 0f;
        isJudged = false;
        isRunning = true;
        frameDuration = shrinkDuration / (shrinkFrames.Length - 1);
        SetFrame(0);
    }

    void Update()
    {
        if (!isRunning || isJudged) return;
        timer += Time.deltaTime;
        int newIndex = Mathf.Clamp(Mathf.FloorToInt(timer / frameDuration), 0, shrinkFrames.Length - 1);

        if (newIndex != currentFrameIndex)
            SetFrame(newIndex);

        // 스페이스바 입력 체크
        if (Input.GetKeyDown(judgeKey))
        {
            TryJudgeByInput();
            return; // 이번 프레임에 판정이 끝났으면 아래 자동 Miss 체크는 건너뜀
        }

        // 목표 프레임을 failFrameRange 이상 지나쳤는데 입력이 없으면 자동 Miss
        if (currentFrameIndex - targetFrameIndex > failFrameRange)
        {
            Judge(JudgeResult.Miss);
        }
        // 마지막 프레임(가장 작은 원)까지 도달했는데도 입력이 없으면 Miss
        else if (currentFrameIndex >= shrinkFrames.Length - 1)
        {
            Judge(JudgeResult.Miss);
        }
    }

    void TryJudgeByInput()
    {
        int diff = Mathf.Abs(currentFrameIndex - targetFrameIndex);

        if (diff <= perfectFrameRange)
            Judge(JudgeResult.Perfect);
        else if (diff <= goodFrameRange)
            Judge(JudgeResult.Good);
        else if (diff <= failFrameRange)
            Judge(JudgeResult.Fail);
        else
            Judge(JudgeResult.Miss);
    }

    void Judge(JudgeResult result)
    {
        isJudged = true;
        isRunning = false;
        OnJudged?.Invoke(this, result);

        // 판정 후 살짝 보이는 시간을 두고 제거 (연출용, 필요 없으면 즉시 Destroy로 변경)
        Destroy(gameObject, 0.15f);
    }

    void SetFrame(int index)
    {
        currentFrameIndex = index;
        Sprite frame = shrinkFrames[index];
        selfImage.sprite = frame;

        // 개별적으로 trim된(크기가 서로 다른) 스프라이트를 쓰는 경우,
        // RectTransform 크기를 스프라이트의 실제 픽셀 크기에 맞춰줘야
        // 늘어나 보이지 않고 실제 크기 그대로 축소되어 보입니다.
        selfRect.sizeDelta = new Vector2(frame.rect.width, frame.rect.height);
    }
}
