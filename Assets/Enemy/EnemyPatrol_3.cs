using UnityEngine;

public class EnemyPatrol_3 : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;

    private int currentIndex = 0;
    private int direction = 1; // 1 = 앞으로, -1 = 뒤로

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];

        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            Flip(target);

            // 끝에 도달하면 방향 반대로
            if (currentIndex == waypoints.Length - 1) direction = -1;
            if (currentIndex == 0) direction = 1;

            currentIndex += direction;
        }
    }

    void Flip(Transform nextTarget)
    {
        Vector3 scale = transform.localScale;
        float dir = nextTarget.position.x - transform.position.x;
        if (dir != 0)
            scale.x = Mathf.Abs(scale.x) * (dir > 0 ? 1 : -1);
        transform.localScale = scale;
    }
}