using UnityEngine;
using TMPro;

public class BossPlayerHealth : MonoBehaviour
{
    public static BossPlayerHealth instance;

    public int maxHp = 5;
    public int currentHp;

    public TextMeshProUGUI hpText; // 없어도 괜찮도록 수정!

    void Awake()
    {
        instance = this;
        ResetHp();
    }

    public void ResetHp()
    {
        currentHp = maxHp;
        UpdateUI();
    }

    public void TakeDamage()
    {
        currentHp--;
        UpdateUI();
        BossManager.instance.OnPlayerHit();
    }

    void UpdateUI()
    {
        // hpText 없어도 에러 안 나도록!
        if (hpText != null)
            hpText.text = "HP : " + currentHp;
    }
}