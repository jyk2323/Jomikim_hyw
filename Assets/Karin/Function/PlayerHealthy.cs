using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    public float maxHp = 100f;
    public float currentHp;

    // 보스전 전용 HP
    public int bossMaxHp = 5;
    public int bossCurrentHp;
    public TextMeshProUGUI bossHpText;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        currentHp = maxHp;
        bossCurrentHp = bossMaxHp;
    }

    void Start()
    {
        currentHp = maxHp;
    }

    // 일반 데미지
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log("현재 HP: " + currentHp);

        if (currentHp <= 0)
        {
            Debug.Log("사망!");
        }
        else
        {
            StartCoroutine(HitEffect());
        }
    }

    // 보스전 데미지
    public void TakeBossDamage()
    {
        bossCurrentHp--;

        if (bossHpText != null)
            bossHpText.text = "HP : " + bossCurrentHp;

        if (BossManager.instance != null)
            BossManager.instance.OnPlayerHit();
    }

    // 보스전 HP 리셋
    public void ResetBossHp()
    {
        bossCurrentHp = bossMaxHp;

        if (bossHpText != null)
            bossHpText.text = "HP : " + bossCurrentHp;
    }

    IEnumerator HitEffect()
    {
        yield return new WaitForSeconds(0.3f);
    }
}