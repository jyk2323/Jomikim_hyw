using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2; // ← 추가

public class PlayerMove : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;

    public bool canMove = true; // ← 추가: TutorialManager가 이 값을 제어

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 이동 잠겨있으면 입력 자체를 안 받음
        if (!canMove)
        {
            moveInput = Vector2.zero;
            animator.SetInteger("Move", 0); // stop 애니메이션 고정
            return;
        }

        // 입력 받기
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // --- 애니메이션 우선순위 제어 ---
        // 1. 좌우 입력이 최우선 (좌우로 움직일 때는 상하 입력을 완전히 무시)
        if (moveInput.x > 0)
        {
            animator.SetInteger("Move", 1); // walk_right
        }
        else if (moveInput.x < 0)
        {
            animator.SetInteger("Move", 2); // walk_left
        }
        // 2. 좌우 입력이 완전히 '0'일 때만 상하 입력 처리
        else if (moveInput.y > 0)
        {
            animator.SetInteger("Move", 3); // walk_back
        }
        else if (moveInput.y < 0)
        {
            animator.SetInteger("Move", 4); // walk_front
        }
        // 3. 아무 키도 안 누를 때만 정지
        else
        {
            animator.SetInteger("Move", 0); // stop
        }
    }

    void FixedUpdate()
    {
        // 이동 잠겨있으면 물리 이동도 즉시 정지
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 유니티 6전용 이동 속도 처리
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}