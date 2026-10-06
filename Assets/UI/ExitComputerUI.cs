using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitComputerUI : MonoBehaviour
{
    public void BackToMainScene()
    {
        SceneManager.LoadScene("2F_Ceo_room");   // 메인 씬 이름 정확히 입력
    }
}