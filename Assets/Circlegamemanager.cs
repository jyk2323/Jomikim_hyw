using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 안 쓰면 이 줄과 아래 TMP_Text 필드는 지우고 UnityEngine.UI.Text로 바꿔주세요.

/// <summary>
/// 축소되는 원 미니게임 전체 흐름(스폰 -> 판정 수신 -> 점수/콤보 -> 게임 종료)을 관리.
/// 
/// 사용 방법:
/// 1. Canvas 하위에 빈 오브젝트(SpawnArea)를 만들고 RectTransform으로 원이 나올 영역을 지정.
/// 2. CircleUnit 프리팹을 만든다:
///    - 부모: CircleUnit (RectTransform)
///      - TargetCircle (Image, 검은 원 스프라이트, 크기 고정 예: 100x100)
///      - ShrinkingCircle (Image, 파란 원 스프라이트, ShrinkCircleController 부착, Raycast Target 체크)
/// 3. 이 스크립트를 빈 오브젝트에 붙이고 인스펙터에 circlePrefab, spawnArea, targetSpriteSize 등을 연결.
/// </summary>
public class CircleGameManager : MonoBehaviour
{
    [Header("프리팹 / 영역")]
    public GameObject circleUnitPrefab; // TargetCircle + ShrinkingCircle 자식을 가진 프리팹
    public RectTransform spawnArea;     // 원이 랜덤하게 나올 영역 (Canvas 하위 패널)

    [Header("스폰 설정")]
    public int totalRounds = 10;        // 총 몇 개의 원을 등장시킬지
    public float spawnInterval = 1.2f;  // 원 사이 등장 간격(초)
    public float unitSize = 150f;       // CircleUnit 프리팹의 가로/세로 크기 (화면 밖으로 안 나가게 여유 공간 계산용)

    [Header("겹침 방지 설정")]
    [Tooltip("이미 떠 있는 원과 이 거리(px)보다 가까우면 다른 위치를 다시 뽑습니다. 보통 unitSize와 비슷하거나 조금 크게.")]
    public float minSpawnDistance = 160f;
    [Tooltip("겹치지 않는 위치를 찾기 위해 최대 몇 번까지 재시도할지")]
    public int maxPlacementAttempts = 30;

    [Header("난이도 (라운드가 진행될수록 점점 빨라지게 하고 싶으면 조절)")]
    public float minShrinkDuration = 0.8f;
    public float maxShrinkDuration = 1.6f;

    [Header("UI (선택 사항, 없으면 비워둬도 됨)")]
    public TMP_Text scoreText;
    public TMP_Text comboText;

    private int currentRound = 0;
    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;

    // 현재 화면에 떠 있는(아직 판정 안 된) 원들의 위치를 기억해서 겹침 체크에 사용
    private Dictionary<ShrinkCircleController, Vector2> activeCircles = new Dictionary<ShrinkCircleController, Vector2>();

    void Start()
    {
        UpdateUI();
        InvokeRepeating(nameof(SpawnNext), 0.5f, spawnInterval);
    }

    void SpawnNext()
    {
        if (currentRound >= totalRounds)
        {
            CancelInvoke(nameof(SpawnNext));
            OnGameEnd();
            return;
        }

        currentRound++;

        GameObject unit = Instantiate(circleUnitPrefab, spawnArea);
        RectTransform unitRect = unit.GetComponent<RectTransform>();

        Vector2 spawnPos = FindNonOverlappingPosition();
        unitRect.anchoredPosition = spawnPos;

        // 프리팹에 이미 shrinkFrames, targetFrameIndex 등이 설정되어 있으므로
        // 여기서는 라운드마다 달라지는 값(축소 속도)만 덮어써줌
        ShrinkCircleController controller = unit.GetComponentInChildren<ShrinkCircleController>();
        controller.shrinkDuration = Random.Range(minShrinkDuration, maxShrinkDuration);
        controller.OnJudged += HandleJudged;

        activeCircles[controller] = spawnPos;

        controller.StartRound();
    }

    /// <summary>
    /// spawnArea 안에서, 이미 떠 있는 원들과 minSpawnDistance 이상 떨어진 위치를 찾아 반환.
    /// maxPlacementAttempts 안에 못 찾으면 마지막으로 뽑은 위치를 그대로 사용(가끔 겹칠 수 있음).
    /// </summary>
    Vector2 FindNonOverlappingPosition()
    {
        Vector2 areaSize = spawnArea.rect.size;
        float margin = unitSize;

        Vector2 candidate = Vector2.zero;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float randX = Random.Range(-areaSize.x / 2f + margin, areaSize.x / 2f - margin);
            float randY = Random.Range(-areaSize.y / 2f + margin, areaSize.y / 2f - margin);
            candidate = new Vector2(randX, randY);

            bool overlapping = false;
            foreach (Vector2 existingPos in activeCircles.Values)
            {
                if (Vector2.Distance(candidate, existingPos) < minSpawnDistance)
                {
                    overlapping = true;
                    break;
                }
            }

            if (!overlapping)
                return candidate; // 겹치지 않는 위치를 찾았으니 바로 반환
        }

        // maxPlacementAttempts 안에 겹치지 않는 자리를 못 찾은 경우 마지막 후보로 진행
        Debug.LogWarning("[CircleGameManager] 겹치지 않는 위치를 찾지 못해 마지막 후보 위치를 사용합니다. minSpawnDistance를 줄이거나 spawnInterval을 늘려보세요.");
        return candidate;
    }

    void HandleJudged(ShrinkCircleController source, ShrinkCircleController.JudgeResult result)
    {
        // 판정이 끝났으니 자리 차지 목록에서 제거 (그 위치에 다시 스폰될 수 있도록)
        activeCircles.Remove(source);

        switch (result)
        {
            case ShrinkCircleController.JudgeResult.Perfect:
                score += 100;
                combo++;
                break;
            case ShrinkCircleController.JudgeResult.Good:
                score += 50;
                combo++;
                break;
            case ShrinkCircleController.JudgeResult.Fail:
            case ShrinkCircleController.JudgeResult.Miss:
                combo = 0;
                break;
        }

        maxCombo = Mathf.Max(maxCombo, combo);
        UpdateUI();

        Debug.Log($"[CircleGame] 판정: {result} / 점수: {score} / 콤보: {combo}");
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (comboText != null) comboText.text = $"Combo: {combo}";
    }

    void OnGameEnd()
    {
        Debug.Log($"[CircleGame] 게임 종료! 최종 점수: {score}, 최고 콤보: {maxCombo}");
        // 여기에 결과 창(Result Panel) 띄우기 등 원하는 종료 처리 추가
    }
}
