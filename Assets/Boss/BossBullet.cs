using UnityEngine;

// 보스전 전용 총알 (1층/2층 로봇 총알은 Enemy/Bullet.cs)
public class BossBullet : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Phase1, Phase2에서 총알을 만든 직후 호출
    // 날아가는 거리 = speed × lifeTime
    public void Init(Vector2 dir, float speed, float lifeTime)
    {
        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // 보스전 데미지 → PlayerHealth → BossManager.OnPlayerHit
        if (PlayerHealth.instance != null)
            PlayerHealth.instance.TakeBossDamage();

        Destroy(gameObject);
    }
}