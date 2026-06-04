using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{   public static PlayerHealth instance; 
    public float maxHp = 100f;
    public float currentHp;

    public Sprite frontSprite;  // 앞모습
    public Sprite backSprite;   // 뒷모습 (피격시)

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frontSprite;
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log("현재 HP: " + currentHp);

        if (currentHp <= 0)
        {
            spriteRenderer.sprite = backSprite; // 죽으면 뒷모습 고정
            Debug.Log("사망!");
        }
        else
        {
            StartCoroutine(HitEffect());
        }
    }

    IEnumerator HitEffect()
    {
        spriteRenderer.sprite = backSprite;         // 뒷모습으로
        yield return new WaitForSeconds(0.3f);      // 0.3초 후
        spriteRenderer.sprite = frontSprite;        // 다시 앞모습
    }

    void Awake()
    {   instance = this; 
        DontDestroyOnLoad(gameObject);
        currentHp = maxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = frontSprite;
    }
}