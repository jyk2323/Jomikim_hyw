using UnityEngine;
using System.Collections;

public class Phase2 : MonoBehaviour
{
    [Tooltip("BossBullet 프리팹을 연결하세요")]
    public GameObject bulletPrefab;

    [Header("발사 위치")]
    public Transform firePoint;

    [Header("총알 설정")]
    public int bulletCount = 10;
    public float bulletSpeed = 5f;
    [Tooltip("총알이 사라지기까지 시간(초). 날아가는 거리 = 속도 × 이 값")]
    public float bulletLifeTime = 3f;
    public float delayBetweenShots = 2f;
    public int repeatCount = 3; // 반복 횟수

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
        for (int i = 0; i < repeatCount; i++)
        {
            FireCurtain();
            yield return new WaitForSeconds(delayBetweenShots);
        }

        // 2차 끝!
        BossManager.instance.OnPhase2End();
    }

    void FireCurtain()
    {
        // X자 커튼 방사형
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (360f / bulletCount) * i;
            float radian = angle * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(
                Mathf.Cos(radian),
                Mathf.Sin(radian)
            );

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
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
}