using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager instance;

    public int currentPhase = 1;
    public int hitCount = 0;
    public int maxHit = 5;

    public Phase1 phase1;
    public Phase2 phase2;

    public string nextSceneName;

    [Header("재시작까지 기다리는 시간(초)")]
    public float restartDelay = 0.5f;

    private bool isRestarting = false;   // 재시작 대기 중인지
    private GameObject followCamera;     // 1층에서 따라온 카메라
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
        isRestarting = true;
        Invoke(nameof(StartPhase1), restartDelay);
    }

    public void StartPhase1()
    {
        isRestarting = false;
        currentPhase = 1;
        hitCount = 0;

        if (PlayerHealth.instance != null)
            PlayerHealth.instance.ResetBossHp();

        phase1.StartPhase();
    }

    public void StartPhase2()
    {
        if (isRestarting) return; // 재시작 중이면 2차로 넘어가지 않음

        currentPhase = 2;
        phase2.StartPhase();
    }

    // 총알에 맞았을 때 (PlayerHealth.TakeBossDamage에서 호출)
    public void OnPlayerHit()
    {
        if (isRestarting) return; // 재시작 대기 중에는 맞아도 무시

        hitCount++;
        if (hitCount >= maxHit)
            Restart();
    }

    // 재시작은 딱 한 번만 예약하고, 남은 것들을 모두 정리
    void Restart()
    {
        isRestarting = true;

        CancelInvoke();      // 이미 예약된 재시작 취소
        phase1.StopPhase();
        phase2.StopPhase();
        ClearBullets();

        Invoke(nameof(StartPhase1), restartDelay);
    }

    // 화면에 남은 총알 전부 삭제
    void ClearBullets()
    {
        Bullet[] bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (Bullet b in bullets)
            Destroy(b.gameObject);
    }

    public void OnPhase2End()
    {
        phase1.StopPhase();
        phase2.StopPhase();
        ClearBullets();

        FadeManager.instance.LoadScene(nextSceneName, Vector2.zero);
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