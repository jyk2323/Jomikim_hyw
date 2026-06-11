using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;

public class SkillCheckManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Range(30f, 360f)]
    public float rotationSpeed = 180f;

    [Range(10f, 60f)]
    public float arcZoneSize = 30f;

    [Range(5f, 20f)]
    public float hitMarkerSize = 10f;

    private GameObject skillCheckRoot;
    private GameObject pointerObj;
    private GameObject arcZoneObj;
    private GameObject hitMarkerObj;

    private bool isActive = false;
    private float currentAngle = 0f;
    private float arcStartAngle = 0f;
    private float hitStartAngle = 0f;

    void Start()
    {
        skillCheckRoot = this.gameObject;
        pointerObj = skillCheckRoot.transform.Find("Pointer").gameObject;
        arcZoneObj = skillCheckRoot.transform.Find("Arc_Zone").gameObject;
        hitMarkerObj = skillCheckRoot.transform.Find("Hit_Marker").gameObject;

        StartSkillCheck();
    }

    void Update()
    {
        // E키 부분 삭제
        if (!isActive) return;

        currentAngle += rotationSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        pointerObj.transform.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            CheckResult();
        }
    }

    public void StartSkillCheck()
    {
        isActive = true;
        skillCheckRoot.SetActive(true);

        // StartSkillCheck() 안에서
        // -arcStartAngle 대신 오프셋 추가해서 맞추기
        arcZoneObj.transform.localRotation = Quaternion.Euler(0f, 0f, -arcStartAngle + 90f); // 90f 조절
        hitMarkerObj.transform.localRotation = Quaternion.Euler(0f, 0f, -hitStartAngle + 90f); // 90f 조절

        arcZoneObj.transform.localRotation = Quaternion.Euler(0f, 0f, -arcStartAngle);
        hitMarkerObj.transform.localRotation = Quaternion.Euler(0f, 0f, -hitStartAngle);

        currentAngle = Random.Range(0f, 360f);
    }

    public void StopSkillCheck()
    {
        isActive = false;
        skillCheckRoot.SetActive(false);
    }

    private void CheckResult()
    {
        float arcEnd = arcStartAngle + arcZoneSize;
        float hitEnd = hitStartAngle + hitMarkerSize;

        bool inArc = IsAngleInRange(currentAngle, arcStartAngle, arcEnd);
        bool inHit = IsAngleInRange(currentAngle, hitStartAngle, hitEnd);

        if (inHit)
        {
            Debug.Log("★ PERFECT! ★");
        }
        else if (inArc)
        {
            Debug.Log("✓ SUCCESS");
        }
        else
        {
            Debug.Log("✗ FAIL");
        }

        StopSkillCheck();
    }

    private bool IsAngleInRange(float angle, float start, float end)
    {
        if (end > 360f)
            return angle >= start || angle <= (end - 360f);
        return angle >= start && angle <= end;
    }
}