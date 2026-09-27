using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    public static BossManager instance;

    public int currentPhase = 1;
    public int hitCount = 0;      // 맞은 횟수
    public int maxHit = 5;        // 최대 5번

    public Phase1 phase1;
    public Phase2 phase2;

    public string nextSceneName;  // 2차 끝나고 이동할 씬

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartPhase1();
    }

    public void StartPhase1()
    {
        currentPhase = 1;
        hitCount = 0;
        BossPlayerHealth.instance.ResetHp();
        phase1.StartPhase();
    }

    public void StartPhase2()
    {
        currentPhase = 2;
        phase2.StartPhase();
    }

    public void OnPlayerHit()
    {
        hitCount++;
        if (hitCount >= maxHit)
        {
            // 5번 맞으면 1차로 리셋!
            StopAllCoroutines();
            phase1.StopPhase();
            phase2.StopPhase();
            StartPhase1();
        }
    }

    public void OnPhase2End()
    {
        // 2차 끝나면 다음 씬으로!
        FadeManager.instance.LoadScene(nextSceneName, Vector2.zero);
    }
}