using UnityEngine;
using UnityEngine.InputSystem;

// 테스트/시연용 씬 바로가기
// - F1 키로 메뉴 열기/닫기 → 버튼 누르면 그 씬으로 바로 이동
// - Canvas나 버튼 만들 필요 없음 (화면 왼쪽 위에 자동으로 그려짐)
// - 1층 씬의 빈 오브젝트에 붙이기 (씬이 바뀌어도 계속 따라다님)
public class SceneJumpMenu : MonoBehaviour
{
    public static SceneJumpMenu instance;

    [System.Serializable]
    public class Jump
    {
        public string buttonName;     // 버튼에 보일 글자 (예: 2층 사무실)
        public string sceneName;      // 씬 이름 (Build Settings에 있어야 함)
        public Vector2 spawnPoint;    // 카린이 설 위치
    }

    [Header("바로가기 목록 (위에서부터 버튼 순서)")]
    public Jump[] jumps;

    [Header("설정")]
    public Key openKey = Key.F1;
    [Tooltip("체크하면 이동할 때 하트도 가득 채움")]
    public bool fullHealOnJump = true;
    [Tooltip("체크하면 Unity 에디터에서만 작동 (완성본 게임에서는 안 열림)")]
    public bool editorOnly = false;

    private bool isOpen = false;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (editorOnly && !Application.isEditor) return;
        if (Keyboard.current != null && Keyboard.current[openKey].wasPressedThisFrame)
            isOpen = !isOpen;
    }

    void OnGUI()
    {
        if (!isOpen) return;

        // 화면 크기에 맞춰 글자/버튼 크기 조절
        float scale = Screen.height / 720f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        GUI.skin.button.fontSize = 18;
        GUI.skin.box.fontSize = 18;

        int count = jumps != null ? jumps.Length : 0;
        GUILayout.BeginArea(new Rect(10, 10, 260, 60 + count * 40));
        GUILayout.Box("씬 바로가기 (" + openKey + " 닫기)");

        for (int i = 0; i < count; i++)
        {
            Jump j = jumps[i];
            string label = string.IsNullOrEmpty(j.buttonName) ? j.sceneName : j.buttonName;
            if (GUILayout.Button(label, GUILayout.Height(34)))
                GoTo(j);
        }
        GUILayout.EndArea();
    }

    void GoTo(Jump j)
    {
        if (string.IsNullOrEmpty(j.sceneName)) return;

        isOpen = false;
        Time.timeScale = 1f;   // 사망 팝업 등으로 멈춰 있었으면 풀기

        // 대사가 나오는 중이면 끝내고 이동 (안 그러면 다음 씬까지 대사창이 따라가서 조작이 막힘)
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive)
        {
            DialogueManager.instance.OnDialogueEnd = null;   // 대사 끝나고 하려던 일(씬 이동 등)은 취소
            DialogueManager.instance.SkipDialogue();
        }

        if (fullHealOnJump && PlayerHealth.instance != null)
            PlayerHealth.instance.HealHearts(PlayerHealth.instance.maxHearts);

        if (FadeManager.instance != null)
            FadeManager.instance.LoadScene(j.sceneName, j.spawnPoint);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(j.sceneName);
    }
}