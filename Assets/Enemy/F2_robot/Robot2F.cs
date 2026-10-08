using UnityEngine;

// 2층 로봇 전용: 순찰 + 방향 판단 + 방향별 스프라이트 + 바라보는 방향으로 발사
// (2층 로봇에서는 EnemyPatrol_3, Shooter를 빼고 이것 하나만 붙인다)
public class Robot2F : MonoBehaviour
{
    public enum Dir { Down, Up, Left, Right }

    [Header("순찰")]
    public Transform[] waypoints;   // 순찰 지점 (왕복)
    public float speed = 2f;

    [Header("방향별 스프라이트 (1장이면 정지 그림, 여러 장이면 애니메이션)")]
    public Sprite[] frontSprites;   // 앞(아래로 갈 때)
    public Sprite[] backSprites;    // 뒤(위로 갈 때)
    public Sprite[] leftSprites;    // 왼쪽
    public Sprite[] rightSprites;   // 오른쪽 (비워두면 왼쪽 그림을 좌우반전)
    public float frameRate = 6f;    // 애니메이션 속도 (초당 장 수)

    [Header("발사")]
    public GameObject bulletPrefab; // 기존 Enemy/Bullet 프리팹
    public float fireRate = 2f;     // 몇 초마다 발사
    public float bulletDamage = 10f;
    public float bulletSpeed = 15f;
    public float bulletLifeTime = 5f;
    public float fireOffset = 0.5f; // 몸 중심에서 얼마나 떨어진 곳에서 총알이 나오는지

    [Header("현재 바라보는 방향 (확인용)")]
    public Dir facing = Dir.Down;

    private SpriteRenderer sr;
    private int currentIndex = 0;
    private int step = 1;           // 1 = 다음 지점, -1 = 이전 지점
    private float fireTimer = 0f;
    private float animTimer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // 예전 방식(Scale X 뒤집기) 흔적 제거: 항상 양수로
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void Start()
    {
        // 시작하자마자 첫 지점 쪽을 바라보게
        if (waypoints.Length > 0)
            UpdateFacing(waypoints[currentIndex].position - transform.position);
        UpdateSprite();
    }

    void Update()
    {
        Patrol();
        UpdateSprite();
        Shoot();
    }

    // ───────── 순찰 ─────────
    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Vector2 target = waypoints[currentIndex].position;
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target) < 0.05f)
        {
            // 끝에 닿으면 반대로 (왕복)
            if (waypoints.Length > 1)
            {
                if (currentIndex == waypoints.Length - 1) step = -1;
                else if (currentIndex == 0) step = 1;
                currentIndex += step;
            }
        }

        // 방향은 "다음에 갈 지점" 기준으로 계산
        UpdateFacing(waypoints[currentIndex].position - transform.position);
    }

    // 움직이는 방향 → 4방향 중 하나 (대각선이면 더 많이 움직이는 쪽)
    void UpdateFacing(Vector2 move)
    {
        if (move.sqrMagnitude < 0.0001f) return; // 거의 안 움직이면 그대로 유지

        if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
            facing = move.x > 0 ? Dir.Right : Dir.Left;
        else
            facing = move.y > 0 ? Dir.Up : Dir.Down;
    }

    // ───────── 스프라이트 ─────────
    void UpdateSprite()
    {
        Sprite[] frames = frontSprites;
        bool flip = false;

        switch (facing)
        {
            case Dir.Down: frames = frontSprites; break;
            case Dir.Up: frames = backSprites; break;
            case Dir.Left: frames = leftSprites; break;
            case Dir.Right:
                if (rightSprites != null && rightSprites.Length > 0)
                    frames = rightSprites;
                else
                {
                    frames = leftSprites; // 오른쪽 그림이 없으면 왼쪽 그림 뒤집기
                    flip = true;
                }
                break;
        }

        sr.flipX = flip;
        if (frames == null || frames.Length == 0) return;

        animTimer += Time.deltaTime;
        int i = (int)(animTimer * frameRate) % frames.Length;
        sr.sprite = frames[i];
    }

    // ───────── 발사 ─────────
    void Shoot()
    {
        if (bulletPrefab == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer < fireRate) return;
        fireTimer = 0f;

        Vector2 dir = FacingVector();
        Vector2 spawnPos = (Vector2)transform.position + dir * fireOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null)
            b.InitBullet(bulletDamage, 0, dir); // 데미지 설정

        // 속도는 여기서 직접 지정 (Bullet.cs 버전과 상관없이 동작)
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = dir * bulletSpeed;

        Destroy(bullet, bulletLifeTime);
    }

    Vector2 FacingVector()
    {
        switch (facing)
        {
            case Dir.Up: return Vector2.up;
            case Dir.Left: return Vector2.left;
            case Dir.Right: return Vector2.right;
            default: return Vector2.down;
        }
    }

    // Scene 화면에서 순찰 경로를 선으로 보여줌 (게임 화면에는 안 보임)
    void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
    }
}