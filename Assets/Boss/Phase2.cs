using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 2차: 네 벽에서 로봇팔이 튀어나옴 → 피하기
// 흐름: 경고(빨간 줄 깜빡) → 팔이 점점 길어짐(그림 1→5) → 잠깐 멈춤 → 다시 짧아짐(5→1)
// 왼쪽/오른쪽 벽 = 가로 그림 / 위/아래 벽 = 세로 그림 (반대쪽 벽은 뒤집어서 사용)
public class Phase2 : BossPhase
{
    [Header("로봇팔 그림 (짧은 것 → 긴 것 순서로!)")]
    [Tooltip("가로 팔: 손이 오른쪽을 보는 그림 5장")]
    public Sprite[] horizontalFrames;
    [Tooltip("세로 팔: 손이 아래를 보는 그림 5장")]
    public Sprite[] verticalFrames;
    [Tooltip("팔 크기 배율 (너무 작거나 크면 조절)")]
    public float armScale = 1f;

    [Header("팔이 그려질 순서 (벽보다 위로)")]
    public string armSortingLayer = "Default";
    public int armSortingOrder = 40;
    [Tooltip("비워두면 기본 머티리얼. 팔이 너무 어두우면 Sprite-Unlit-Default 넣기")]
    public Material armMaterial;

    [Header("싸움 영역 (안쪽 벽 기준) - Scene 창의 노란 네모")]
    public Vector2 arenaMin = new Vector2(-11f, -6f);
    public Vector2 arenaMax = new Vector2(11f, 6f);

    [Tooltip("팔이 모서리에 너무 붙지 않게 띄우는 거리 (팔 두께와 별도로 추가)")]
    public float cornerPadding = 0.5f;

    [Header("웨이브")]
    public int waveCount = 6;             // 몇 번 공격할지
    public int armsFirstWave = 1;         // 첫 웨이브 팔 개수
    public int armsLastWave = 3;          // 마지막 웨이브 팔 개수 (점점 늘어남)
    public float waveInterval = 1.5f;     // 웨이브 사이 간격
    public float armGap = 0.2f;           // 같은 웨이브 안에서 팔끼리 시간차
    [Range(0f, 1f)]
    [Tooltip("카린 위치를 노리고 나올 확률 (0 = 완전 랜덤)")]
    public float targetPlayerChance = 0.5f;

    [Header("경고 표시")]
    [Tooltip("가로 경고 그림: 오른쪽을 보는 그림 (왼쪽 벽용, 오른쪽 벽은 뒤집어서 씀). 비우면 빨간 네모")]
    public Sprite warningHorizontal;
    [Tooltip("세로 경고 그림: 아래를 보는 그림 (위 벽용, 아래 벽은 뒤집어서 씀). 비우면 빨간 네모")]
    public Sprite warningVertical;
    [Tooltip("체크: 팔이 닿는 범위에 맞춰 늘림 / 해제: 그림 원래 크기로 범위 가운데에 표시")]
    public bool stretchWarning = false;
    [Tooltip("경고 그림 크기 배율 (늘리지 않을 때만)")]
    public float warningSpriteScale = 1f;
    [Tooltip("경고 그림 색. 흰색 = 그림 원래 색 그대로")]
    public Color warningSpriteColor = Color.white;
    public float warningTime = 0.8f;
    public float warningBlink = 0.1f;
    public Color warningColor = new Color(1f, 0f, 0f, 0.35f);
    public int warningSortingOrder = 39;
    [Tooltip("경고가 안 보이거나 어두우면 Sprite-Unlit-Default 넣기")]
    public Material warningMaterial;

    [Header("팔 움직임 (초)")]
    public float extendTime = 0.25f;      // 1→5 길어지는 시간
    public float holdTime = 0.5f;         // 다 나와서 멈춰있는 시간
    public float retractTime = 0.4f;      // 5→1 짧아지는 시간
    [Range(0.3f, 1f)]
    [Tooltip("맞는 범위를 그림보다 조금 작게 (1 = 그림 크기 그대로)")]
    public float hitboxScale = 0.8f;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private int activeArms = 0;
    private static Sprite whiteSprite;

    protected override IEnumerator PhaseRoutine()
    {
        if (horizontalFrames == null || horizontalFrames.Length == 0 ||
            verticalFrames == null || verticalFrames.Length == 0)
        {
            Debug.LogWarning("[Phase2] 로봇팔 그림(Horizontal/Vertical Frames)이 비어있음 → 2차를 건너뜀");
            yield break;
        }

        for (int w = 0; w < waveCount; w++)
        {
            float t = waveCount <= 1 ? 1f : (float)w / (waveCount - 1);
            int arms = Mathf.RoundToInt(Mathf.Lerp(armsFirstWave, armsLastWave, t));

            List<int> usedSides = new List<int>();
            for (int i = 0; i < arms; i++)
            {
                StartCoroutine(ArmRoutine(PickSide(usedSides)));
                yield return new WaitForSeconds(armGap);
            }

            yield return new WaitForSeconds(waveInterval);
        }

        // 마지막 팔들이 다 들어갈 때까지 기다림
        while (activeArms > 0) yield return null;
    }

    // 0 왼쪽벽, 1 오른쪽벽, 2 아래벽, 3 위벽 (같은 웨이브에선 가능하면 다른 벽)
    int PickSide(List<int> used)
    {
        int side = Random.Range(0, 4);
        for (int k = 0; k < 4 && used.Contains(side); k++) side = (side + 1) % 4;
        used.Add(side);
        return side;
    }

    IEnumerator ArmRoutine(int side)
    {
        activeArms++;

        bool horizontal = side <= 1;
        Sprite[] frames = horizontal ? horizontalFrames : verticalFrames;
        // 팔이 나오는 방향 (벽 → 안쪽)
        Vector2 dir = side == 0 ? Vector2.right : side == 1 ? Vector2.left : side == 2 ? Vector2.up : Vector2.down;

        // 팔 오브젝트 만들기 (반대쪽 벽은 그림 뒤집기)
        GameObject arm = new GameObject("RobotArm");
        arm.transform.localScale = Vector3.one * armScale;
        SpriteRenderer sr = arm.AddComponent<SpriteRenderer>();
        if (armMaterial != null) sr.material = armMaterial;
        sr.sortingLayerName = armSortingLayer;
        sr.sortingOrder = armSortingOrder;
        sr.flipX = side == 1;   // 가로 그림은 오른쪽을 봄 → 오른쪽 벽이면 뒤집기
        sr.flipY = side == 2;   // 세로 그림은 아래를 봄 → 아래 벽이면 뒤집기
        spawned.Add(arm);

        // 가장 긴 그림으로 팔 두께 재기
        sr.sprite = frames[frames.Length - 1];
        float halfThick = (horizontal ? sr.bounds.size.y : sr.bounds.size.x) / 2f + cornerPadding;

        // 벽 위의 어느 위치에서 나올지 (카린 노리기 or 랜덤)
        // 팔 두께 절반만큼 안쪽으로 → 팔이 노란 네모 밖(모서리)으로 삐져나가지 않음
        float lo = (horizontal ? arenaMin.y : arenaMin.x) + halfThick;
        float hi = (horizontal ? arenaMax.y : arenaMax.x) - halfThick;
        if (lo > hi) lo = hi = (lo + hi) / 2f;

        Vector3 player;
        bool hasPlayer = TryGetPlayer(out player);
        float along;
        if (hasPlayer && Random.value < targetPlayerChance)
            along = horizontal ? player.y : player.x;
        else
            along = Random.Range(lo, hi);
        along = Mathf.Clamp(along, lo, hi);

        // 팔 뿌리가 붙는 벽 위치
        Vector2 wallPoint = side == 0 ? new Vector2(arenaMin.x, along)
                          : side == 1 ? new Vector2(arenaMax.x, along)
                          : side == 2 ? new Vector2(along, arenaMin.y)
                          : new Vector2(along, arenaMax.y);

        // 가장 긴 그림 크기 = 경고 범위
        PlaceArm(arm, sr, wallPoint, dir);
        Bounds longest = sr.bounds;
        sr.enabled = false;

        // 1) 경고: 팔이 나올 자리를 빨갛게 깜빡
        GameObject warn = MakeWarning(side, wallPoint, dir, longest.center, longest.size, sr.sortingLayerName);
        spawned.Add(warn);
        SpriteRenderer warnSr = warn.GetComponent<SpriteRenderer>();
        float timer = 0f;
        while (timer < warningTime)
        {
            warnSr.enabled = !warnSr.enabled;
            yield return new WaitForSeconds(warningBlink);
            timer += warningBlink;
        }
        spawned.Remove(warn);
        Destroy(warn);

        // 2) 길어짐 → 3) 멈춤 → 4) 짧아짐 (닿으면 피격, 팔 하나당 1번만)
        sr.enabled = true;
        bool hit = false;
        System.Func<bool> alreadyHit = () => hit;
        System.Action setHit = () => hit = true;

        int last = frames.Length - 1;
        // 길어지기: 0,1,2,3,4
        for (int i = 0; i <= last; i++)
            yield return ShowFrame(arm, sr, frames[i], wallPoint, dir, extendTime / frames.Length, alreadyHit, setHit);
        // 멈춤
        yield return ShowFrame(arm, sr, frames[last], wallPoint, dir, holdTime, alreadyHit, setHit);
        // 짧아지기: 4,3,2,1,0
        for (int i = last; i >= 0; i--)
            yield return ShowFrame(arm, sr, frames[i], wallPoint, dir, retractTime / frames.Length, alreadyHit, setHit);

        spawned.Remove(arm);
        Destroy(arm);
        activeArms--;
    }

    // 그림 한 장을 time초 동안 보여주면서 카린과 닿는지 검사
    IEnumerator ShowFrame(GameObject arm, SpriteRenderer sr, Sprite frame, Vector2 wallPoint, Vector2 dir,
                          float time, System.Func<bool> alreadyHit, System.Action setHit)
    {
        sr.sprite = frame;
        PlaceArm(arm, sr, wallPoint, dir);

        float t = 0f;
        do
        {
            if (!alreadyHit() && TouchesPlayer(sr.bounds))
            {
                setHit();
                if (PlayerHealth.instance != null) PlayerHealth.instance.TakeBossDamage();
            }
            t += Time.deltaTime;
            yield return null;
        } while (t < time);
    }

    // 그림의 벽쪽 끝이 벽 선에 딱 붙도록 위치 맞추기 (그림 pivot이 어디든 상관없음)
    void PlaceArm(GameObject arm, SpriteRenderer sr, Vector2 wallPoint, Vector2 dir)
    {
        Bounds b = sr.bounds;
        Vector3 offset = b.center - arm.transform.position;
        Vector2 wantCenter = wallPoint + dir * ((Mathf.Abs(dir.x) > 0 ? b.size.x : b.size.y) / 2f);
        arm.transform.position = (Vector3)wantCenter - offset;
    }

    bool TouchesPlayer(Bounds b)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, (Vector2)b.size * hitboxScale, 0f);
        foreach (Collider2D c in hits)
            if (c.CompareTag("Player")) return true;
        return false;
    }

    bool TryGetPlayer(out Vector3 pos)
    {
        pos = Vector3.zero;
        GameObject p = PlayerHealth.instance != null ? PlayerHealth.instance.gameObject : GameObject.FindWithTag("Player");
        if (p == null) return false;
        pos = p.transform.position;
        return true;
    }

    // 경고용 반투명 빨간 네모 (따로 그림 필요 없음)
    GameObject MakeWarning(int side, Vector2 wallPoint, Vector2 dir, Vector3 center, Vector3 size, string layer)
    {
        // 왼쪽/오른쪽 벽 = 가로 그림, 위/아래 벽 = 세로 그림 (로봇팔과 같은 방식)
        Sprite warningSprite = side <= 1 ? warningHorizontal : warningVertical;

        GameObject g = new GameObject("ArmWarning");
        g.transform.position = new Vector3(center.x, center.y, 0f);
        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        if (warningMaterial != null) sr.material = warningMaterial;

        if (warningSprite != null)
        {
            // 직접 만든 경고 그림
            sr.sprite = warningSprite;
            sr.color = warningSpriteColor;
            sr.flipX = side == 1;   // 오른쪽 벽이면 좌우 뒤집기
            sr.flipY = side == 2;   // 아래 벽이면 상하 뒤집기
            if (stretchWarning)
            {
                Vector2 spriteSize = warningSprite.bounds.size;
                g.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
            }
            else
            {
                g.transform.localScale = Vector3.one * warningSpriteScale;
            }
        }
        else
        {
            // 그림이 없으면 빨간 네모
            if (whiteSprite == null)
                whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            sr.sprite = whiteSprite;
            sr.color = warningColor;
            g.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        sr.sortingLayerName = layer;
        sr.sortingOrder = warningSortingOrder;

        // 경고 그림의 벽쪽 끝을 벽에 딱 붙이기 (팔과 같은 방식, 그림 중심점이 어디든 상관없음)
        if (warningSprite != null)
            PlaceArm(g, sr, wallPoint, dir);
        return g;
    }

    // 재시작 / 다음 차수로 갈 때 남은 팔·경고 전부 정리
    protected override void OnStopPhase()
    {
        StopAllCoroutines();
        foreach (GameObject g in spawned)
            if (g != null) Destroy(g);
        spawned.Clear();
        activeArms = 0;
    }

    // Scene 창에 싸움 영역 표시 (Phase2Object 선택했을 때)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((arenaMin + arenaMax) / 2f, arenaMax - arenaMin);
    }
}