using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

// 카린 체력 + 하트 표시 + 사망 연출(빨간 깜빡임 → 검은 화면 → 팝업) + 다시 시작/끝내기
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;

    [Header("체력 (하트 1개 = maxHp / maxHearts)")]
    public float maxHp = 100f;
    public float currentHp;
    public int maxHearts = 10;

    [Header("하트 표시 (1층 Canvas 안 텍스트)")]
    public TextMeshProUGUI heartText;
    public string[] hideHeartsInScenes = { "Start", "Opening", "Part1,2", "ComputerUI" };

    [Header("사망 연출 - 빨간 깜빡임")]
    public Color blinkColor = new Color(1f, 0f, 0f, 1f);   // 색과 투명도(A) 조절
    public int blinkCount = 4;                              // 깜빡이는 횟수
    public float blinkInterval = 0.12f;                     // 깜빡임 간격(초)

    [Header("사망 연출 - 검은 화면")]
    public Image blackScreen;                               // 화면 전체 Image
    public Color blackColor = new Color(0f, 0f, 0f, 0.85f); // 색과 투명도(A) 조절
    public float blackFadeTime = 1f;                        // 어두워지는 시간(초)

    [Header("사망 팝업")]
    public GameObject deathPopup;                           // 문구 + 버튼 2개가 있는 패널
    public string restartSceneName = "1F_Scene";
    public string startSceneName = "Start";

    [Header("이 씬들에서는 카린, 인벤토리 UI, 따라오는 카메라를 숨김")]
    public string[] hideWorldInScenes = { "Start", "Opening" };

    // 보스전 전용 HP
    public int bossMaxHp = 5;
    public int bossCurrentHp;
    public TextMeshProUGUI bossHpText;

    private bool isDead = false;
    private Vector2 respawnPosition;     // 1층 처음 시작 위치
    private SpriteRenderer sr;
    private Color originalColor = Color.white;
    private bool backToSpawn = false;    // 시작화면에 갔다 오면 1층 처음 위치에서

    public bool IsDead => isDead;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        currentHp = maxHp;
        bossCurrentHp = bossMaxHp;

        respawnPosition = transform.position;   // 처음 놓인 자리 = 다시 시작 위치
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        SceneManager.sceneLoaded += OnSceneLoaded;

        HideDeathUI();
        if (blackScreen == null) Debug.LogWarning("[PlayerHealth] Black Screen 칸이 비어 있어요. 1층 씬의 KARIN에 연결하세요.");
        if (deathPopup == null) Debug.LogWarning("[PlayerHealth] Death Popup 칸이 비어 있어요. 1층 씬의 KARIN에 연결하세요.");
    }

    void Start()
    {
        currentHp = maxHp;
        HideDeathUI();
        UpdateHearts();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 시작화면/오프닝에서는 숨기고, 나머지 씬에서는 보이기
        bool hideWorld = System.Array.IndexOf(hideWorldInScenes, scene.name) >= 0;
        SetWorldVisible(!hideWorld);

        // 시작화면에 갔다가 1층으로 돌아오면 처음 위치로
        if (backToSpawn && scene.name == restartSceneName)
        {
            backToSpawn = false;
            transform.position = respawnPosition;
        }

        // 죽은 상태가 아니면 사망 화면은 항상 숨김
        if (!isDead) HideDeathUI();

        UpdateHearts();
    }

    void SetWorldVisible(bool visible)
    {
        if (gameObject.activeSelf != visible) gameObject.SetActive(visible);

        if (Inventory.instance != null)
        {
            Canvas c = Inventory.instance.GetComponent<Canvas>();
            if (c != null) c.enabled = visible;
        }

        if (CameraFollow.instance != null)
            CameraFollow.instance.gameObject.SetActive(visible);
    }

    // ───────── 일반 데미지 ─────────
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp = Mathf.Max(0f, currentHp - damage);
        UpdateHearts();

        if (currentHp <= 0)
            StartCoroutine(DeathRoutine());
    }

    // 하트 = 남은 HP를 하트 1개 크기로 나눈 값 (올림)
    void UpdateHearts()
    {
        if (heartText == null) return;

        bool hide = System.Array.IndexOf(hideHeartsInScenes, SceneManager.GetActiveScene().name) >= 0;
        heartText.gameObject.SetActive(!hide);
        if (hide) return;

        float perHeart = maxHp / maxHearts;
        int hearts = Mathf.Clamp(Mathf.CeilToInt(currentHp / perHeart), 0, maxHearts);
        heartText.text = new string('♥', hearts) + new string('♡', maxHearts - hearts);
    }

    // ───────── 사망 연출 ─────────
    IEnumerator DeathRoutine()
    {
        isDead = true;
        SetCanMove(false);

        // 1) 빨간색으로 깜빡이기
        for (int i = 0; i < blinkCount; i++)
        {
            if (sr != null) sr.color = blinkColor;
            yield return new WaitForSecondsRealtime(blinkInterval);
            if (sr != null) sr.color = originalColor;
            yield return new WaitForSecondsRealtime(blinkInterval);
        }
        if (sr != null) sr.color = blinkColor;

        // 2) 화면 전체 검게 덮기
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.raycastTarget = true;   // 뒤쪽 UI 클릭 막기
            float t = 0f;
            Color from = new Color(blackColor.r, blackColor.g, blackColor.b, 0f);
            while (t < blackFadeTime)
            {
                t += Time.unscaledDeltaTime;
                blackScreen.color = Color.Lerp(from, blackColor, t / blackFadeTime);
                yield return null;
            }
            blackScreen.color = blackColor;
        }

        // 3) 게임 멈추고 팝업
        Time.timeScale = 0f;
        if (deathPopup != null) deathPopup.SetActive(true);
    }

    // 사망 상태 풀기 (HP 회복, 색 복구, 움직임 허용)
    void Revive()
    {
        Time.timeScale = 1f;
        HideDeathUI();

        isDead = false;
        currentHp = maxHp;
        if (sr != null) sr.color = originalColor;
        SetCanMove(true);
        UpdateHearts();
    }

    // 다시 시작 버튼 OnClick → 1층 처음 위치에서 (아이템·진행 상황은 유지)
    public void Restart()
    {
        Revive();

        if (FadeManager.instance != null)
            FadeManager.instance.LoadScene(restartSceneName, respawnPosition);
        else
        {
            transform.position = respawnPosition;
            SceneManager.LoadScene(restartSceneName);
        }
    }

    // 시작화면 버튼 OnClick → Start 씬으로 (초기화 안 함: 아이템·진행 상황 유지)
    // 다시 시작하기를 누르면 오프닝 → 1층 처음 위치에서 이어서 진행
    public void GoToStart()
    {
        Revive();
        backToSpawn = true;
        SceneManager.LoadScene(startSceneName);
    }

    void HideDeathUI()
    {
        if (deathPopup != null) deathPopup.SetActive(false);
        if (blackScreen != null) blackScreen.gameObject.SetActive(false);
    }

    void SetCanMove(bool value)
    {
        PlayerMove move = GetComponent<PlayerMove>();
        if (move != null) move.canMove = value;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null && !value) rb.linearVelocity = Vector2.zero;
    }

    // ───────── 보스전 ─────────
    public void TakeBossDamage()
    {
        bossCurrentHp--;

        if (bossHpText != null)
            bossHpText.text = "HP : " + bossCurrentHp;

        if (BossManager.instance != null)
            BossManager.instance.OnPlayerHit();
    }

    public void ResetBossHp()
    {
        bossCurrentHp = bossMaxHp;

        if (bossHpText != null)
            bossHpText.text = "HP : " + bossCurrentHp;
    }
}