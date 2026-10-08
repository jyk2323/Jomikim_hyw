using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject bulletPrefab; // 총알 프리팹 연결
    public float fireRate = 2f;     // 몇 초마다 발사할지 (2 = 2초마다)

    void Start()
    {
        // 게임 시작하자마자 fireRate 간격으로 Fire() 반복 호출
        InvokeRepeating("Fire", 0f, fireRate);
    }

    void Fire()
    {
        // 캐릭터가 보는 방향 감지
        float direction = transform.localScale.x > 0 ? 1f : -1f;

        // 총알 생성 (내 위치에서 생성)
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

       // 방향에 맞게 발사
        bullet.GetComponent<Bullet>().InitBullet(10f, 0, new Vector3(direction, 0, 0));
    }
}