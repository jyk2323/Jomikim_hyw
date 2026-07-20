using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{   
    public static PlayerHealth instance; 
    public float maxHp = 100f;
    public float currentHp;

    void Awake()
    {   
        // 이미 KARIN이 존재하면 새로 생긴 거 삭제!
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

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
        yield return new WaitForSeconds(0.3f);
    }
}