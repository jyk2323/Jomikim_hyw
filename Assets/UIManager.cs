using UnityEngine;

public class UIManager : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectsByType<UIManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}