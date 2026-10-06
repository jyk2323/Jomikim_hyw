using UnityEngine;
using System.Collections;

public class BossManager : MonoBehaviour
{
    public static BossManager instance;

    public int currentPhase = 1;
    public int hitCount = 0;
    public int maxHit = 5;

    public Phase1 phase1;
    public Phase2 phase2;

    public string nextSceneName;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Invoke("StartPhase1", 0.5f);
    }

    public void StartPhase1()
    {
        currentPhase = 1;
        hitCount = 0;
        PlayerHealth.instance.ResetBossHp(); // 수정!
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
            StopAllCoroutines();
            phase1.StopPhase();
            phase2.StopPhase();
            Invoke("StartPhase1", 0.5f);
        }
    }

    public void OnPhase2End()
    {
        FadeManager.instance.LoadScene(nextSceneName, Vector2.zero);
    }
}