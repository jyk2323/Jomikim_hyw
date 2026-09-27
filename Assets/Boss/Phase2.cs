using UnityEngine;
using System.Collections;

public class Phase2 : MonoBehaviour
{
    public GameObject bulletPrefab;

    [Header("발사 위치")]
    public Transform firePoint;

    [Header("총알 설정")]
    public int bulletCount = 10;
    public float bulletSpeed = 5f;
    public float delayBetweenShots = 2f;
    public int repeatCount = 3; // 반복 횟수

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
            bullet.GetComponent<Bullet>().InitBullet(1f, 0, dir);
        }
    }
}