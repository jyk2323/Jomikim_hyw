using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;   // ← 추가: 씬 바뀜 감지
using Vector2 = UnityEngine.Vector2; // ← 추가

public class PlayerMove : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;

    public bool canMove = true; // ← 추가: TutorialManager가 이 값을 제어

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput; 

    // ← 추가: 마지막으로 걸은 방향 (멈췄을 때 그 방향을 보고 서 있게)
    // Move 값: 0 앞 대기 / 1 오른쪽 걷기 / 2 왼쪽 걷기 / 3 뒤 걷기 / 4 앞 걷기
    //          5 오른쪽 대기 / 6 왼쪽 대기 / 7 뒤 대기
    private int lastWalk = 4;   // 처음엔 앞을 보고 시작

    // ← 추가: 걷는 소리
    [Header("발소리 (볼륨은 시작화면 효과음 슬라이더를 따름)")]
    [Tooltip("발소리 파일 (여러 개 넣으면 번갈아 무작위로 재생)")]
    public AudioClip[] footstepSounds;
    [Tooltip("발소리 간격(초). 걷는 애니메이션에 맞춰 조절")]
    public float stepInterval = 0.35f;
    [Range(0f, 1f)] public float footstepVolume = 0.6f;
    [Range(0f, 0.3f)] public float footstepPitchRandom = 0.08f;
    private AudioSource footAudio;
    private float stepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // ← 추가: 발소리 전용 스피커
        footAudio = gameObject.AddComponent<AudioSource>();
        footAudio.playOnAwake = false;
    }

    // ← 추가: 씬이 바뀌면 항상 정면을 보고 시작
    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastWalk = 4;                                   // 앞 걷기 → 멈추면 앞 대기
        stepTimer = 0f;
        if (animator != null) animator.SetInteger("Move", 0);
    }

    // 움직일 수 있는지: 튜토리얼 잠금(canMove)도 아니고, 대사 중도 아닐 때만
    bool CanControl()
    {
        if (!canMove) return false;
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive) return false;
        return true;
    }

    void Update()
    {
        // 이동 잠겨있으면 입력 자체를 안 받음 (튜토리얼 또는 대사 중)
        if (!CanControl())
        {
            moveInput = Vector2.zero;
            animator.SetInteger("Move", IdleOf(lastWalk)); // ← 수정: 보던 방향 그대로 대기
            stepTimer = 0f;   // ← 추가
            return;
        }

        // 입력 받기
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // --- 애니메이션 우선순위 제어 ---
        // 1. 좌우 입력이 최우선 (좌우로 움직일 때는 상하 입력을 완전히 무시)
        if (moveInput.x > 0)
        {
            lastWalk = 1;
            animator.SetInteger("Move", 1); // walk_right
        }
        else if (moveInput.x < 0)
        {
            lastWalk = 2;
            animator.SetInteger("Move", 2); // walk_left
        }
        // 2. 좌우 입력이 완전히 '0'일 때만 상하 입력 처리
        else if (moveInput.y > 0)
        {
            lastWalk = 3;
            animator.SetInteger("Move", 3); // walk_back
        }
        else if (moveInput.y < 0)
        {
            lastWalk = 4;
            animator.SetInteger("Move", 4); // walk_front
        }
        // 3. 아무 키도 안 누를 때: 마지막으로 걸은 방향을 보고 대기 (← 수정)
        else
        {
            animator.SetInteger("Move", IdleOf(lastWalk));
        }

        // ← 추가: 걷는 동안 일정 간격으로 발소리 (걷기 시작하는 순간 바로 한 번)
        if (moveInput != Vector2.zero)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    // ← 추가: 발소리 한 번
    void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || footAudio == null) return;
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        if (clip == null) return;
        footAudio.pitch = 1f + Random.Range(-footstepPitchRandom, footstepPitchRandom);
        footAudio.PlayOneShot(clip, footstepVolume * AudioManager.SfxVolume);
    }

    // ← 추가: 걷던 방향 → 그 방향 대기 번호
    int IdleOf(int walk)
    {
        switch (walk)
        {
            case 1: return 5;   // 오른쪽 걷기 → 오른쪽 대기
            case 2: return 6;   // 왼쪽 걷기 → 왼쪽 대기
            case 3: return 7;   // 뒤 걷기 → 뒤 대기
            default: return 0;  // 앞 걷기 → 앞 대기 (원래 stop)
        }
    }

    void FixedUpdate()
    {
        // 이동 잠겨있으면 물리 이동도 즉시 정지
        if (!CanControl())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 유니티 6전용 이동 속도 처리
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}