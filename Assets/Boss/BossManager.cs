using UnityEngine;
using TMPro;
using System.Collections;

// 보스전 전체 진행 관리
// - 차수는 Inspector의 Phases 목록 순서대로 진행 (순서 바꾸려면 목록에서 끌어서 바꾸기)
// - 하트를 다 잃으면 "지금 차수"부터 다시 시작
// - 마지막 차수가 끝나면 다음 씬으로 이동
public class BossManager : MonoBehaviour
{
    public static BossManager instance;

    [Header("차수 목록 (위에서부터 1차, 2차, 3차...)")]
    public BossPhase[] phases;

    [Tooltip("테스트용: 몇 차부터 시작할지 (1 = 처음부터). 완성되면 1로 되돌리기")]
    public int startPhase = 1;

    [Tooltip("한 차수가 끝나고 다음 차수 시작까지 쉬는 시간(초)")]
    public float phaseGap = 1f;

    [Header("하트")]
    public int maxHit = 5;
    public int hitCount = 0;

    [Header("보스전 끝나면 이동할 씬")]
    public string nextSceneName;
    public Vector2 nextSpawnPoint;

    [Header("재시작까지 기다리는 시간(초)")]
    public float restartDelay = 1.5f;

    [Header("HP 표시 (이 씬의 Canvas 안 텍스트)")]
    public TextMeshProUGUI hpText;
    [Tooltip("체크: ♥♥♥♡♡ 하트로 / 해제: HP : 3 숫자로")]
    public bool useHearts = true;

    [Header("재시작 안내 (비워두면 안 띄움)")]
    public TextMeshProUGUI restartText;   // 화면 가운데 큰 글자
    public string restartMessage = "다시 시작!";

    [Header("확인용 (지금 몇 번째 차수인지, 0부터)")]
    public int currentIndex = 0;

    private bool isRestarting = false;    // 재시작/차수 전환 대기 중인지
    private bool isEnding = false;
    private GameObject followCamera;      // 1층에서 따라온 카메라
    private bool isQuitting = false;

    void Awake()
    {
        instance = this;

        // 보스전은 이 씬의 고정 카메라를 사용 → 따라오는 카메라는 잠시 끈다
        CameraFollow follow = FindAnyObjectByType<CameraFollow>();
        if (follow != null)
        {
            followCamera = follow.gameObject;
            followCamera.SetActive(false);
        }
    }

    void Start()
    {
        UpdateHpText();
        if (restartText != null) restartText.gameObject.SetActive(false);

        currentIndex = Mathf.Clamp(startPhase - 1, 0, Mathf.Max(0, phases.Length - 1));
        isRestarting = true;
        Invoke(nameof(BeginCurrentPhase), restartDelay);
    }

    // ───────── 차수 진행 ─────────
    void BeginCurrentPhase()
    {
        isRestarting = false;
        if (restartText != null) restartText.gameObject.SetActive(false);

        // 목록에 빈 칸(아직 안 만든 차수)이 있으면 건너뜀
        while (currentIndex < phases.Length && phases[currentIndex] == null)
            currentIndex++;

        if (currentIndex >= phases.Length)
        {
            EndBoss();
            return;
        }

        phases[currentIndex].StartPhase();
    }

    // 각 차수가 끝나면 (끝 대사까지 끝난 뒤) BossPhase가 부름
    public void OnPhaseEnd(BossPhase phase)
    {
        if (isRestarting || isEnding) return;
        if (currentIndex >= phases.Length || phases[currentIndex] != phase) return;

        ClearBullets();
        currentIndex++;

        isRestarting = true; // 다음 차수 시작 전까지 맞아도 무시
        Invoke(nameof(BeginCurrentPhase), phaseGap);
    }

    void EndBoss()
    {
        if (isEnding) return;
        isEnding = true;

        StopAllPhases();
        ClearBullets();
        FadeManager.instance.LoadScene(nextSceneName, nextSpawnPoint);
    }

    // ───────── 피격 / 재시작 ─────────
    // 총알, 로봇팔에 맞았을 때 (PlayerHealth.TakeBossDamage에서 호출)
    public void OnPlayerHit()
    {
        if (isRestarting || isEnding) return;
        // 차수 사이 대사 중에는 안 맞음
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive) return;

        hitCount++;
        UpdateHpText();

        if (hitCount >= maxHit)
            Restart();
    }

    // 하트를 다 잃으면 지금 차수를 처음부터 다시 (하트도 다시 가득)
    void Restart()
    {
        isRestarting = true;

        CancelInvoke();
        StopAllPhases();
        ClearBullets();

        hitCount = 0;
        if (PlayerHealth.instance != null) PlayerHealth.instance.ResetBossHp();

        if (restartText != null)
        {
            restartText.text = restartMessage;
            restartText.gameObject.SetActive(true);
        }

        Invoke(nameof(RefillAndBegin), restartDelay);
    }

    void RefillAndBegin()
    {
        UpdateHpText();
        BeginCurrentPhase();
    }

    void StopAllPhases()
    {
        foreach (BossPhase p in phases)
            if (p != null) p.StopPhase();
    }

    // 남은 HP = 최대 - 맞은 횟수
    void UpdateHpText()
    {
        if (hpText == null) return;

        int hp = Mathf.Max(0, maxHit - hitCount);

        if (useHearts)
            hpText.text = new string('♥', hp) + new string('♡', maxHit - hp);
        else
            hpText.text = "HP : " + hp;
    }

    // 화면에 남은 총알 전부 삭제
    public void ClearBullets()
    {
        BossBullet[] bullets = FindObjectsByType<BossBullet>(FindObjectsSortMode.None);
        foreach (BossBullet b in bullets)
            Destroy(b.gameObject);
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // 보스전 씬을 떠날 때 따라오는 카메라를 다시 켠다
    void OnDestroy()
    {
        if (!isQuitting && followCamera != null)
            followCamera.SetActive(true);

        if (instance == this)
            instance = null;
    }
}

// ───────── 모든 차수의 공통 규칙 ─────────
// 새 차수를 만들 때는 BossPhase를 상속하고 PhaseRoutine()만 만들면 됨.
// PhaseRoutine이 끝나면 → (끝 대사) → BossManager.OnPhaseEnd → 다음 차수
public abstract class BossPhase : MonoBehaviour
{
    [Header("이 차수가 끝나고 나오는 대사 (비우면 바로 다음 차수)")]
    public Sprite panelSprite;     // 대사창 배경 (비우면 기본)
    [TextArea]
    public string[] endLines;      // 줄마다 맨 앞에 [이름]

    private Coroutine routine;

    public void StartPhase()
    {
        StopPhase();
        routine = StartCoroutine(Run());
    }

    public void StopPhase()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        OnStopPhase();
    }

    // 차수의 실제 패턴 (각 차수 스크립트가 만듦)
    protected abstract IEnumerator PhaseRoutine();

    // 멈출 때 정리할 것이 있으면 (로봇팔 지우기 등)
    protected virtual void OnStopPhase() { }

    IEnumerator Run()
    {
        yield return PhaseRoutine();
        routine = null;

        if (BossManager.instance != null) BossManager.instance.ClearBullets();

        if (endLines != null && endLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.OnDialogueEnd = () => BossManager.instance.OnPhaseEnd(this);
            DialogueManager.instance.StartDialogue(endLines, true, panelSprite, null);
        }
        else if (BossManager.instance != null)
        {
            BossManager.instance.OnPhaseEnd(this);
        }
    }
}