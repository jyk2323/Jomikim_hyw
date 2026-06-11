using UnityEngine;

public class SettingsPopup : MonoBehaviour
{
    public GameObject settingsPanel; // 팝업 패널 연결

    // 설정 버튼 눌렀을 때
    public void OpenSettings()
    {
        settingsPanel.SetActive(true); // 패널 보이기
    }

    // 닫기 버튼 눌렀을 때
    public void CloseSettings()
    {
        settingsPanel.SetActive(false); // 패널 숨기기
    }
}
