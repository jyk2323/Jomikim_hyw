using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;

    public Transform target;

    void Awake()
    {
        // 이미 따라다니는 카메라가 있으면 새로 생긴 건 삭제 (1층에 다시 올 때마다 카메라가 늘어나는 것 방지)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드될 때마다 실행!
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
        // 씬 바뀔 때마다 KARIN 다시 찾아서 연결!
        if (PlayerHealth.instance != null)
        {
            target = PlayerHealth.instance.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            -10f
        );
    }
}