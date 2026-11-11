using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingLoader : MonoBehaviour
{
    [SerializeField] private string endingSceneName = "Ending_Scene";   // 컷신 씬 이름
    [SerializeField] private float delay = 1.5f;   // 로딩 100% 화면을 보여줄 시간

    private bool loading;

    // 이벤트(On Hack Complete)에 연결할 함수
    public void LoadEnding()
    {
        if (loading) return;
        loading = true;
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 1f;   // 멈춰 있었다면 되돌리기
        SceneManager.LoadScene(endingSceneName);
    }
}