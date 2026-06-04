using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = dir * speed;
    }
}