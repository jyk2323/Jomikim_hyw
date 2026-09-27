using UnityEngine;
using System.Collections;

public class Phase1 : MonoBehaviour
{
    public GameObject bulletPrefab;  // 기존 Bullet 프리팹

    [Header("발사 위치 (Inspector에서 조절!)")]
    public Transform topCenter;      // 상단 중앙
    public Transform bottomLeft;     // 하단 좌측
    public Transform bottomRight;    // 하단 우측

    [Header("총알 설정")]
    public int bulletCount = 7;          // 총알 개수
    public float bulletSpeed = 5f;       // 총알 속도
    public float delayBetweenShots = 2f; // 발사 간격

    private Coroutine phaseCoroutine;

    public void StartPhase()
    {
        phaseCoroutine = StartCoroutine(PhaseRoutine());
    }

    public void StopPhase()
    {
        if (phaseCoroutine != null)
            StopCoroutine(phaseCoroutine);
    }

    IEnumerator PhaseRoutine()
    {
        // 상단 중앙에서 발사
        FireSemiCircle(topCenter.position, Vector2.down);
        yield return new WaitForSeconds(delayBetweenShots);

        // 하단 좌측에서 발사
        FireSemiCircle(bottomLeft.position, Vector2.up);
        yield return new WaitForSeconds(delayBetweenShots);

        // 하단 우측에서 발사
        FireSemiCircle(bottomRight.position, Vector2.up);
        yield return new WaitForSeconds(delayBetweenShots);

        // 1차 끝 → 대사 출력 후 2차로!
        DialogueManager.instance.OnDialogueEnd = () => BossManager.instance.StartPhase2();
        DialogueManager.instance.StartDialogue(
            "카린", null, new string[] { "여기서 끝낼 순 없어!" }, true, null, null);
    }

    void FireSemiCircle(Vector2 position, Vector2 baseDir)
    {
        // 반원 방향으로 총알 발사!
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -90f + (180f / (bulletCount - 1)) * i;
            float radian = angle * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(
                baseDir.x * Mathf.Cos(radian) - baseDir.y * Mathf.Sin(radian),
                baseDir.x * Mathf.Sin(radian) + baseDir.y * Mathf.Cos(radian)
            );

            GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
            bullet.GetComponent<Bullet>().InitBullet(1f, 0, dir);
        }
    }
}