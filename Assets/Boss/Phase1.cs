using UnityEngine;
using System.Collections;

public class Phase1 : MonoBehaviour
{
    [Tooltip("BossBullet 프리팹을 연결하세요")]
    public GameObject bulletPrefab;

    [Header("발사 위치 (Inspector에서 조절!)")]
    public Transform topCenter;
    public Transform bottomLeft;
    public Transform bottomRight;

    [Header("부채꼴 모양 (각도는 0=오른쪽, 90=위, 180=왼쪽, -90=아래)")]
    [Tooltip("총알이 퍼지는 전체 폭(도). 180 = 반원, 90 = 좁은 부채꼴, 360 = 원")]
    [Range(0f, 360f)] public float spreadAngle = 180f;
    [Tooltip("위쪽 발사 위치가 가운데로 쏘는 방향")]
    public float topCenterAim = -90f;
    [Tooltip("왼쪽 아래 발사 위치가 가운데로 쏘는 방향 (예: 45 = 오른쪽 위 대각선)")]
    public float bottomLeftAim = 90f;
    [Tooltip("오른쪽 아래 발사 위치가 가운데로 쏘는 방향 (예: 135 = 왼쪽 위 대각선)")]
    public float bottomRightAim = 90f;

    [Header("총알 설정")]
    public int bulletCount = 7;
    public float bulletSpeed = 5f;
    [Tooltip("총알이 사라지기까지 시간(초). 날아가는 거리 = 속도 × 이 값")]
    public float bulletLifeTime = 3f;
    public float delayBetweenShots = 2f;

    [Header("섞어 쏘기 (각각 한 번씩 쏜 다음)")]
    [Tooltip("섞어서 몇 번 더 쏠지 (매번 3곳 중 랜덤 1곳, 같은 곳 연속 X)")]
    public int mixedShotCount = 5;
    [Tooltip("섞어 쏘기 사이 간격(초)")]
    public float mixedDelay = 1.2f;

    [Header("1차 끝나고 대사")]
    public Sprite panelSprite;   // 대사창 배경 (비우면 기본)

    [TextArea]
    public string[] endLines;    // 줄마다 맨 앞에 [이름]. 예) [카린] 여기서 끝낼 순 없어!

    private Coroutine phaseCoroutine;

    public void StartPhase()
    {
        StopPhase(); // 이전에 돌던 패턴이 있으면 먼저 멈춤 (중복 실행 방지)
        phaseCoroutine = StartCoroutine(PhaseRoutine());
    }

    public void StopPhase()
    {
        if (phaseCoroutine != null)
        {
            StopCoroutine(phaseCoroutine);
            phaseCoroutine = null;
        }
    }

    IEnumerator PhaseRoutine()
    {
        FireSemiCircle(topCenter.position, topCenterAim);
        yield return new WaitForSeconds(delayBetweenShots);

        FireSemiCircle(bottomLeft.position, bottomLeftAim);
        yield return new WaitForSeconds(delayBetweenShots);

        FireSemiCircle(bottomRight.position, bottomRightAim);
        yield return new WaitForSeconds(delayBetweenShots);

        // 섞어 쏘기: 매번 3곳 중 한 곳에서만
        int last = -1;
        for (int n = 0; n < mixedShotCount; n++)
        {
            int pick = Random.Range(0, 3);
            if (pick == last) pick = (pick + Random.Range(1, 3)) % 3; // 같은 곳 연속 방지
            last = pick;
            FireFromPoint(pick);

            yield return new WaitForSeconds(mixedDelay);
        }

        // 대사 있으면 출력 후 2차로, 없으면 바로 2차로!
        if (endLines != null && endLines.Length > 0)
        {
            DialogueManager.instance.OnDialogueEnd = () => BossManager.instance.StartPhase2();
            DialogueManager.instance.StartDialogue(endLines, true, panelSprite, null);
        }
        else
        {
            BossManager.instance.StartPhase2();
        }
    }

    // 0 = 위쪽, 1 = 왼쪽 아래, 2 = 오른쪽 아래
    void FireFromPoint(int index)
    {
        if (index == 0) FireSemiCircle(topCenter.position, topCenterAim);
        else if (index == 1) FireSemiCircle(bottomLeft.position, bottomLeftAim);
        else FireSemiCircle(bottomRight.position, bottomRightAim);
    }

    // i번째 총알의 방향 계산: 가운데 방향(aim)을 기준으로 spreadAngle 폭만큼 고르게 나눔
    Vector2 BulletDir(float aim, int i)
    {
        float angle;
        if (bulletCount <= 1)
            angle = aim;
        else if (spreadAngle >= 360f)
            angle = aim + (360f / bulletCount) * i;            // 원: 끝과 끝이 겹치지 않게
        else
            angle = aim - spreadAngle / 2f + (spreadAngle / (bulletCount - 1)) * i;

        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void FireSemiCircle(Vector2 position, float aim)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 dir = BulletDir(aim, i);

            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            BossBullet bossBullet = bullet.GetComponent<BossBullet>();
            if (bossBullet == null)
            {
                Debug.LogError("Bullet Prefab에 BossBullet 스크립트가 없어요! BossBullet 프리팹을 연결하세요.");
                Destroy(bullet);
                return;
            }
            bossBullet.Init(dir, bulletSpeed, bulletLifeTime);
        }
    }

    // Scene 창에서 발사 방향을 선으로 미리 보여줌 (게임 화면에는 안 보임)
    void OnDrawGizmos()
    {
        DrawFan(topCenter, topCenterAim, Color.red);
        DrawFan(bottomLeft, bottomLeftAim, Color.yellow);
        DrawFan(bottomRight, bottomRightAim, Color.cyan);
    }

    void DrawFan(Transform point, float aim, Color color)
    {
        if (point == null) return;
        Gizmos.color = color;
        for (int i = 0; i < bulletCount; i++)
            Gizmos.DrawLine(point.position, (Vector2)point.position + BulletDir(aim, i) * 2f);
    }
}