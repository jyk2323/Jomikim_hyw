using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 가로 막대(인디케이터)가 트랙 위를 좌우로 왕복하다가,
/// 플레이어가 입력한 시점에 랜덤 목표 구간 안에 있으면 성공, 아니면 실패.
/// 
/// 씬 구성 예시 (모두 RectTransform, 앵커/피벗은 (0.5, 0.5) 권장):
///  - TrackBar    : 배경 역할의 긴 회색 막대 (전체 이동 범위)
///  - TargetZone  : TrackBar 자식으로 배치, 원래 그려진 크기 그대로 사용하고 위치만 랜덤하게 갱신되는 목표 구간
///  - CriticalZone: **TargetZone의 자식**으로 배치! 타겟존 안쪽에서만 랜덤 위치로 갱신되는 크리티컬 판정 구간
///  - Indicator   : TrackBar 자식으로 배치, 좌우로 이동하는 작은 마커 (크기는 에디터에서 정한 그대로 유지)
///
/// TargetZone, CriticalZone, Indicator는 인스펙터에서 미리 원하는 크기(Width/Height)로 세팅해두세요.
/// 스크립트는 이 오브젝트들의 크기를 절대 건드리지 않고, 위치(anchoredPosition.x)만 변경합니다.
/// </summary>
public class HorizontalBarMiniGame : MonoBehaviour
{
    [Header("UI 참조")]
    public RectTransform trackBar;
    public RectTransform targetZone;
    public RectTransform criticalZone;   // TargetZone의 자식으로 배치! (같이 이동함)
    public RectTransform indicator;

    [Header("이동 설정")]
    [Tooltip("인디케이터 이동 속도 (px/sec)")]
    public float moveSpeed = 300f;

    [Header("입력 설정")]
    public KeyCode confirmKey = KeyCode.Space;
    public bool useMouseClick = true;

    [Header("결과 이벤트")]
    public UnityEvent onCriticalSuccess; // 크리티컬 존 안에서 멈췄을 때
    public UnityEvent onSuccess;         // 크리티컬은 아니지만 타겟존 안에서 멈췄을 때
    public UnityEvent onFail;

    private float trackHalfWidth;
    private float indicatorHalfWidth;
    private float moveDirection = 1f;
    private bool isPlaying = false;

    private float targetMinX;
    private float targetMaxX;
    private float criticalMinX;
    private float criticalMaxX;

    void Start()
    {
        StartGame();
    }

    /// <summary>
    /// 게임(라운드) 시작. 버튼 등에서 재시작 시 호출 가능.
    /// </summary>
    public void StartGame()
    {
        trackHalfWidth = trackBar.rect.width / 2f;
        indicatorHalfWidth = indicator.rect.width / 2f;

        SetRandomTargetZone();

        // 인디케이터를 트랙 왼쪽 끝에서 시작
        Vector2 pos = indicator.anchoredPosition;
        pos.x = -trackHalfWidth + indicatorHalfWidth;
        indicator.anchoredPosition = pos;

        moveDirection = 1f;
        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        MoveIndicator();

        bool confirmPressed = Input.GetKeyDown(confirmKey) ||
                               (useMouseClick && Input.GetMouseButtonDown(0));

        if (confirmPressed)
        {
            CheckResult();
        }
    }

    private void MoveIndicator()
    {
        Vector2 pos = indicator.anchoredPosition;
        pos.x += moveDirection * moveSpeed * Time.deltaTime;

        float leftLimit = -trackHalfWidth + indicatorHalfWidth;
        float rightLimit = trackHalfWidth - indicatorHalfWidth;

        if (pos.x >= rightLimit)
        {
            pos.x = rightLimit;
            moveDirection = -1f;
        }
        else if (pos.x <= leftLimit)
        {
            pos.x = leftLimit;
            moveDirection = 1f;
        }

        indicator.anchoredPosition = pos;
    }

    private void SetRandomTargetZone()
    {
        // 너비는 인스펙터에서 미리 설정해둔 그대로 사용 (스크립트가 크기를 바꾸지 않음)
        float targetWidth = targetZone.rect.width;

        float minX = -trackHalfWidth + targetWidth / 2f;
        float maxX = trackHalfWidth - targetWidth / 2f;
        float centerX = Random.Range(minX, maxX);

        // 위치만 랜덤하게 반영
        Vector2 pos = targetZone.anchoredPosition;
        pos.x = centerX;
        targetZone.anchoredPosition = pos;

        targetMinX = centerX - targetWidth / 2f;
        targetMaxX = centerX + targetWidth / 2f;

        SetRandomCriticalZone(targetWidth);
    }

    /// <summary>
    /// 크리티컬 존을 TargetZone 안쪽 범위에서 랜덤 위치로 배치.
    /// criticalZone은 targetZone의 자식이므로, anchoredPosition은 targetZone 기준 상대 좌표입니다.
    /// </summary>
    private void SetRandomCriticalZone(float targetWidth)
    {
        float criticalWidth = criticalZone.rect.width;

        // targetZone 로컬 기준 이동 가능 범위 (크리티컬 존이 타겟존 밖으로 나가지 않도록 제한)
        float localMin = -targetWidth / 2f + criticalWidth / 2f;
        float localMax = targetWidth / 2f - criticalWidth / 2f;
        float localCenterX = Random.Range(localMin, localMax);

        Vector2 pos = criticalZone.anchoredPosition;
        pos.x = localCenterX;
        criticalZone.anchoredPosition = pos;

        // 판정은 trackBar 기준 좌표로 통일해야 하므로, targetZone의 위치를 더해 절대 좌표로 변환
        float absoluteCenterX = targetZone.anchoredPosition.x + localCenterX;
        criticalMinX = absoluteCenterX - criticalWidth / 2f;
        criticalMaxX = absoluteCenterX + criticalWidth / 2f;
    }

    private void CheckResult()
    {
        isPlaying = false;

        float indicatorX = indicator.anchoredPosition.x;

        if (indicatorX >= criticalMinX && indicatorX <= criticalMaxX)
        {
            Debug.Log("크리티컬 성공!");
            onCriticalSuccess?.Invoke();
        }
        else if (indicatorX >= targetMinX && indicatorX <= targetMaxX)
        {
            Debug.Log("성공!");
            onSuccess?.Invoke();
        }
        else
        {
            Debug.Log("실패!");
            onFail?.Invoke();
        }
    }
}