using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA; // 왼쪽 끝
    public Transform pointB; // 오른쪽 끝
    public float speed = 2f; // 이동 속도

    private Transform target; // 현재 목표 지점

    void Start()
    {
        target = pointB; // 처음엔 B쪽으로 이동 시작
    }

    void Update()
    {
        // 목표 지점으로 이동
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // 목표 지점에 도착하면 반대쪽으로 전환
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA;
            Flip(); // 스프라이트 좌우 반전
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1; // x값 반전 → 좌우 뒤집기
        transform.localScale = scale;
    }
}