using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("1F_Scene"); // 이동할 씬 이름 입력
    }
}