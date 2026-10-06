using UnityEngine;
using System.Collections;

public class Phase1 : MonoBehaviour
{
    public GameObject bulletPrefab;

    [Header("발사 위치 (Inspector에서 조절!)")]
    public Transform topCenter;
    public Transform bottomLeft;
    public Transform bottomRight;

    [Header("총알 설정")]
    public int bulletCount = 7;
    public float bulletSpeed = 5f;
    public float delayBetweenShots = 2f;

    [Header("1차 끝나고 대사")]
    public string speakerName;
    public Sprite speakerImage;
    public Sprite panelSprite;

    [TextArea]
    public string[] endLines; // Inspector에서 입력!

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
        FireSemiCircle(topCenter.position, Vector2.down);
        yield return new WaitForSeconds(delayBetweenShots);

        FireSemiCircle(bottomLeft.position, Vector2.up);
        yield return new WaitForSeconds(delayBetweenShots);

        FireSemiCircle(bottomRight.position, Vector2.up);
        yield return new WaitForSeconds(delayBetweenShots);

        // 대사 있으면 출력 후 2차로, 없으면 바로 2차로!
        if (endLines != null && endLines.Length > 0)
        {
            DialogueManager.instance.OnDialogueEnd = () => BossManager.instance.StartPhase2();
            DialogueManager.instance.StartDialogue(speakerName, speakerImage, endLines, true, panelSprite, null);
        }
        else
        {
            BossManager.instance.StartPhase2();
        }
    }

    void FireSemiCircle(Vector2 position, Vector2 baseDir)
    {
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