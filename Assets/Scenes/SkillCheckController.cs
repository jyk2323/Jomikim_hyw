using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 데바데 스타일 스킬 체크 미니게임 컨트롤러
/// - 원형 링 위를 포인터가 회전
/// - 성공 구간(초록)과 완벽 구간(흰색)이 존재
/// - 스페이스바 또는 지정 키로 판정
/// </summary>
public class SkillCheckController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("스킬 체크 전체 패널 (루트 오브젝트)")]
    public GameObject skillCheckPanel;

    [Tooltip("회전하는 포인터 이미지 (Pivot을 중심으로 회전)")]
    public RectTransform pointer;

    [Tooltip("성공 구간을 표시하는 Image (Image Type: Filled, Fill Method: Radial360)")]
    public Image successZoneImage;

    [Tooltip("완벽(Great) 구간을 표시하는 Image (동일 설정)")]
    public Image perfectZoneImage;

    [Header("Skill Check Settings")]
    [Tooltip("포인터 회전 속도 (도/초). 높을수록 어려워짐")]
    [Range(60f, 720f)]
    public float rotationSpeed = 180f;

    [Tooltip("성공 구간의 각도 크기 (도)")]
    [Range(10f, 90f)]
    public float successZoneAngle = 40f;

    [Tooltip("완벽 구간의 각도 크기 (도) - 성공 구간 안에 위치")]
    [Range(5f, 30f)]
    public float perfectZoneAngle = 10f;

    [Tooltip("판정 입력 키")]
    public KeyCode inputKey = KeyCode.Space;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip perfectSound;
    public AudioClip failSound;

    // 내부 상태
    private float currentAngle = 0f;         // 현재 포인터 각도 (0~360)
    private float successZoneStart = 0f;     // 성공 구간 시작 각도
    private float perfectZoneStart = 0f;     // 완벽 구간 시작 각도
    private bool isActive = false;

    // 결과 이벤트
    public event Action<SkillCheckResult> OnSkillCheckComplete;

    public enum SkillCheckResult { Miss, Success, Perfect }

    // ─────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────

    /// <summary>스킬 체크를 시작합니다.</summary>
    public void StartSkillCheck(float speed = -1f)
    {
        if (speed > 0f) rotationSpeed = speed;

        // 구간을 랜덤 배치 (포인터 시작점에서 충분히 앞에 위치)
        PlaceZonesRandomly();

        currentAngle = 0f;
        isActive = true;
        skillCheckPanel.SetActive(true);

        UpdateZoneVisuals();
        UpdatePointerVisual();
    }

    /// <summary>스킬 체크를 강제로 닫습니다.</summary>
    public void CancelSkillCheck()
    {
        isActive = false;
        skillCheckPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        if (skillCheckPanel != null)
            skillCheckPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // 포인터 회전
        currentAngle += rotationSpeed * Time.deltaTime;

        // 360도를 넘으면 → Miss (한 바퀴 다 돌았으면 실패)
        if (currentAngle >= 360f)
        {
            Resolve(SkillCheckResult.Miss);
            return;
        }

        UpdatePointerVisual();

        // 입력 감지
        if (Input.GetKeyDown(inputKey))
        {
            Judge();
        }
    }

    // ─────────────────────────────────────────────
    //  Core Logic
    // ─────────────────────────────────────────────

    private void Judge()
    {
        // 완벽 구간 체크 (우선)
        if (IsInZone(currentAngle, perfectZoneStart, perfectZoneAngle))
        {
            Resolve(SkillCheckResult.Perfect);
        }
        // 성공 구간 체크
        else if (IsInZone(currentAngle, successZoneStart, successZoneAngle))
        {
            Resolve(SkillCheckResult.Success);
        }
        else
        {
            Resolve(SkillCheckResult.Miss);
        }
    }

    private void Resolve(SkillCheckResult result)
    {
        isActive = false;
        skillCheckPanel.SetActive(false);

        PlaySound(result);
        OnSkillCheckComplete?.Invoke(result);

        Debug.Log($"[SkillCheck] 결과: {result} | 포인터: {currentAngle:F1}° | 성공구간: {successZoneStart:F1}°~{successZoneStart + successZoneAngle:F1}°");
    }

    /// <summary>각도가 구간 안에 있는지 확인 (0~360 래핑 지원)</summary>
    private bool IsInZone(float angle, float zoneStart, float zoneSize)
    {
        float end = (zoneStart + zoneSize) % 360f;
        if (zoneStart < end)
            return angle >= zoneStart && angle <= end;
        else // 360도 경계를 넘는 경우
            return angle >= zoneStart || angle <= end;
    }

    /// <summary>성공/완벽 구간을 랜덤 위치에 배치</summary>
    private void PlaceZonesRandomly()
    {
        // 포인터가 0도에서 시작하므로, 적어도 45도 앞에 구간 배치
        float minStart = 45f;
        float maxStart = 360f - successZoneAngle - 10f;
        successZoneStart = UnityEngine.Random.Range(minStart, maxStart);

        // 완벽 구간은 성공 구간 안 중앙 근처에 배치
        float innerOffset = (successZoneAngle - perfectZoneAngle) * UnityEngine.Random.Range(0.1f, 0.9f);
        perfectZoneStart = (successZoneStart + innerOffset) % 360f;
    }

    // ─────────────────────────────────────────────
    //  Visuals
    // ─────────────────────────────────────────────

    /// <summary>포인터 RectTransform 회전 업데이트</summary>
    private void UpdatePointerVisual()
    {
        if (pointer == null) return;
        // Unity UI에서 Z축 회전: 0도=위쪽, 시계방향이 되도록 음수
        pointer.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
    }

    /// <summary>성공/완벽 구간 Image(Filled) 시각 업데이트</summary>
    private void UpdateZoneVisuals()
    {
        SetFilledZone(successZoneImage, successZoneStart, successZoneAngle);
        SetFilledZone(perfectZoneImage, perfectZoneStart, perfectZoneAngle);
    }

    /// <summary>
    /// Image Type=Filled, Fill Method=Radial360, Fill Origin=Top 기준으로
    /// 특정 각도 범위에 아크를 그립니다.
    /// Unity의 Radial360은 한 방향으로만 채우기 때문에,
    /// RectTransform 회전으로 시작점을 조정합니다.
    /// </summary>
    private void SetFilledZone(Image img, float startAngle, float arcAngle)
    {
        if (img == null) return;

        img.fillAmount = arcAngle / 360f;
        // fillOrigin=Top(0도=위), 시계방향 기준으로 startAngle만큼 회전
        img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -startAngle);
    }

    // ─────────────────────────────────────────────
    //  Audio
    // ─────────────────────────────────────────────

    private void PlaySound(SkillCheckResult result)
    {
        if (audioSource == null) return;
        AudioClip clip = result switch
        {
            SkillCheckResult.Perfect => perfectSound,
            SkillCheckResult.Success => successSound,
            _ => failSound
        };
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
