using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;

    private Rigidbody2D rb;  // 이름 rb로 변경

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InitBullet(float damage, int per, Vector3 dir)
    {
        this.damage = damage;
        this.per = per;
        float bulletSpeed = 15f;

        if (per > -1)
        {
            rb.linearVelocity = dir * bulletSpeed;
        }
    }

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        collision.GetComponent<PlayerHealth>().TakeDamage(damage);
        Destroy(gameObject);
    }
}