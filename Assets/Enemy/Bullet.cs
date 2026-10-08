using UnityEngine;

// 1층/2층 로봇 총알 (보스 총알은 Boss/BossBullet.cs)
public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // speed, lifeTime을 안 넣으면 속도 15, 5초 후 삭제
    public void InitBullet(float damage, int per, Vector3 dir, float speed = 15f, float lifeTime = 5f)
    {
        this.damage = damage;
        this.per = per;

        if (per > -1)
        {
            rb.linearVelocity = dir.normalized * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
            player.TakeDamage(damage);

        Destroy(gameObject);
    }
}