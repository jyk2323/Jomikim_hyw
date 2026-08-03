using UnityEngine;
using UnityEngine.EventSystems;

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

        // EventSystem 없으면 새로 만들기!
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            DontDestroyOnLoad(eventSystem);
        }
        else
        {
            DontDestroyOnLoad(FindAnyObjectByType<EventSystem>().gameObject);
        }
    }
}