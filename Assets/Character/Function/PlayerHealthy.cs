using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{   
    public static PlayerHealth instance; 
    public float maxHp = 100f;
    public float currentHp;

    void Awake()
    {   
        instance = this; 
        DontDestroyOnLoad(gameObject);
        currentHp = maxHp;
    }

    void Start()
    {
        currentHp = maxHp;
    }

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

    IEnumerator HitEffect()
    {
        // 스프라이트 변경 코드는 삭제됨
        // 나중에 깜빡임 효과 등 다른 피격 효과를 넣으려면 이곳에 코드를 추가하세요.
        yield return new WaitForSeconds(0.3f);      // 0.3초 대기
    }
}