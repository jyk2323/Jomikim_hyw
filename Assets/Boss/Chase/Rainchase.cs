using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// 추격전: 레인이 카린을 쫓아옴
// - 대사가 끝나고 잠깐 뒤에 출발 (카린이 먼저 도망갈 시간)
// - 카린 쪽으로 똑바로 걸어옴, 시간이 갈수록 조금씩 빨라짐
// - 잡히면 → 추격전 처음부터 다시 / 출구에 닿으면 → 다음 씬
// - 움직이는 방향에 맞춰 걷는 그림을 바꿔줌 (Animator 필요 없음)
// 레인 오브젝트에 붙이기 (SpriteRenderer 필요)
public class RainChase : MonoBehaviour
{
    [Header("걷는 그림 (각 4장, 순서대로)")]
    public Sprite[] frontFrames;   // 아래로 걸을 때 (Rain_Front_walk)
    public Sprite[] backFrames;    // 위로 걸을 때 (Rain_Back_walk)
    [Tooltip("왼쪽으로 걸을 때. 비우면 옆으로 갈 때도 앞/뒷모습 사용")]
    public Sprite[] leftFrames;
    [Tooltip("오른쪽으로 걸을 때. 비우면 왼쪽 그림을 뒤집어서 사용")]
    public Sprite[] rightFrames;
    public Sprite[] idleFrames;    // 서 있을 때 (Rain_Stop_front)
    [Tooltip("옆모습이 나오는 기준. 1 = 가로로 조금만 더 가도 옆모습 / 클수록 확실히 옆으로 갈 때만")]
    public float sideBias = 1f;
    [Tooltip("그림 한 장이 보이는 시간(초). 작을수록 빨리 걷는 느낌")]
    public float frameTime = 0.12f;

    [Header("속도 (카린 속도는 5)")]
    public float moveSpeed = 3.5f;
    [Tooltip("1초마다 빨라지는 양 (0이면 계속 같은 속도)")]
    public float speedUpPerSecond = 0.1f;
    public float maxSpeed = 5.5f;

    [Header("출발")]
    [Tooltip("대사가 끝나고 몇 초 뒤에 쫓아올지")]
    public float startDelay = 1.5f;

    [Header("잡혔을 때")]
    [Tooltip("이 거리보다 가까워지면 잡힘")]
    public float catchDistance = 0.7f;
    public TextMeshProUGUI caughtText;           // 비워도 됨
    public string caughtMessage = "잡혔다...";
    public float restartDelay = 1.5f;
    [Tooltip("다시 시작할 때 카린이 설 위치")]
    public Vector2 playerRestartPoint;

    [Header("출구 (Scene 창의 초록 원)")]
    public Transform goalPoint;
    public float goalRadius = 1f;
    public string nextSceneName;
    public Vector2 nextSpawnPoint;

    private SpriteRenderer sr;
    private Transform player;
    private bool chasing = false;
    private bool finished = false;   // 잡혔거나 탈출함
    private float currentSpeed;
    private float animTimer;
    private int frameIndex;
    private Sprite[] currentFrames;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (caughtText != null) caughtText.gameObject.SetActive(false);
    }

    IEnumerator Start()
    {
        currentSpeed = moveSpeed;
        SetFrames(idleFrames, false);

        // 카린 찾기
        while (player == null)
        {
            if (PlayerHealth.instance != null) player = PlayerHealth.instance.transform;
            else
            {
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
            yield return null;
        }

        // 씬 시작 대사가 끝날 때까지 기다림 (대사가 바로 뜨는 경우를 위해 한 프레임 더)
        yield return null;
        while (DialogueBusy()) yield return null;

        yield return new WaitForSeconds(startDelay);
        chasing = true;
    }

    void Update()
    {
        if (player == null || finished) { Animate(); return; }

        // 출구 도착 확인
        if (goalPoint != null && Vector2.Distance(player.position, goalPoint.position) <= goalRadius)
        {
            Escape();
            return;
        }

        // 대사 중이거나 카린이 죽었으면 멈춤
        if (!chasing || DialogueBusy() || PlayerDead())
        {
            SetFrames(idleFrames, false);
            Animate();
            return;
        }

        // 점점 빨라짐
        currentSpeed = Mathf.Min(maxSpeed, currentSpeed + speedUpPerSecond * Time.deltaTime);

        // 카린 쪽으로 이동
        Vector2 toPlayer = player.position - transform.position;
        transform.position = Vector2.MoveTowards(transform.position, player.position, currentSpeed * Time.deltaTime);
        ChooseFrames(toPlayer);
        Animate();

        // 잡힘
        if (toPlayer.magnitude <= catchDistance)
            StartCoroutine(Caught());
    }

    // 움직이는 방향에 맞는 그림 고르기
    void ChooseFrames(Vector2 dir)
    {
        bool hasLeft = leftFrames != null && leftFrames.Length > 0;
        bool hasRight = rightFrames != null && rightFrames.Length > 0;
        // 세로보다 가로로 더 많이 움직일 때 = 옆모습
        bool sideways = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) * sideBias && (hasLeft || hasRight);

        if (sideways && dir.x > 0)
        {
            if (hasRight) SetFrames(rightFrames, false);
            else SetFrames(leftFrames, true);       // 오른쪽 그림 없으면 왼쪽 그림 뒤집기
        }
        else if (sideways)
        {
            if (hasLeft) SetFrames(leftFrames, false);
            else SetFrames(rightFrames, true);      // 왼쪽 그림 없으면 오른쪽 그림 뒤집기
        }
        else if (dir.y > 0)
            SetFrames(backFrames, false);       // 위로 → 뒷모습
        else
            SetFrames(frontFrames, false);      // 아래로 → 앞모습
    }

    void SetFrames(Sprite[] frames, bool flip)
    {
        if (frames == null || frames.Length == 0) return;
        sr.flipX = flip;
        if (frames == currentFrames) return;
        currentFrames = frames;
        frameIndex = 0;
        animTimer = 0f;
        sr.sprite = frames[0];
    }

    void Animate()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;
        animTimer += Time.deltaTime;
        if (animTimer >= frameTime)
        {
            animTimer = 0f;
            frameIndex = (frameIndex + 1) % currentFrames.Length;
            sr.sprite = currentFrames[frameIndex];
        }
    }

    IEnumerator Caught()
    {
        finished = true;
        SetFrames(idleFrames, false);

        // 카린 못 움직이게
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        PlayerMove move = player.GetComponent<PlayerMove>();
        if (move != null) move.enabled = false;

        if (caughtText != null)
        {
            caughtText.text = caughtMessage;
            caughtText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(restartDelay);

        if (move != null) move.enabled = true;
        // 이 씬을 처음부터 다시 (레인도 원래 자리로 돌아감)
        FadeManager.instance.LoadScene(SceneManager.GetActiveScene().name, playerRestartPoint);
    }

    void Escape()
    {
        finished = true;
        SetFrames(idleFrames, false);
        FadeManager.instance.LoadScene(nextSceneName, nextSpawnPoint);
    }

    bool DialogueBusy()
    {
        return DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive;
    }

    bool PlayerDead()
    {
        return PlayerHealth.instance != null && PlayerHealth.instance.IsDead;
    }

    // Scene 창 표시: 빨간 원 = 잡히는 거리 / 초록 원 = 출구 / 파란 점 = 카린 재시작 위치
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        if (goalPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goalPoint.position, goalRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(playerRestartPoint, 0.2f);
    }
}