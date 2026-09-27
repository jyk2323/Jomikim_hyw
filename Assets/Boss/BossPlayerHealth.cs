using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossPlayerHealth : MonoBehaviour
{
    public static BossPlayerHealth instance;

    public int maxHp = 5;
    public int currentHp;

    public TextMeshProUGUI hpText; // HP 표시 UI (선택사항)

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
        if (hpText != null)
            hpText.text = "HP : " + currentHp;
    }
}