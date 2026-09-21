using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public GameObject documentsPanel;
    public GameObject editorPanel;
    public GameObject documentViewPanel;

    void Start()
    {
        // 시작 시 창 3개는 모두 꺼두고, 바탕화면만 보이게
        documentsPanel.SetActive(false);
        editorPanel.SetActive(false);
        documentViewPanel.SetActive(false);
    }

    // ① 폴더 클릭 → 문서 목록 열기
    public void OpenDocuments()
    {
        documentsPanel.SetActive(true);
    }

    // ② 문서 아이콘 클릭 → 편집기 열기 (문서목록 닫기)
    public void OpenEditor()
    {
        editorPanel.SetActive(true);
        documentsPanel.SetActive(false);
    }

    // ③ "최근 사용 열기" 클릭 → 워드 문서 열기 (편집기 닫기)
    public void OpenDocument()
    {
        documentViewPanel.SetActive(true);
        editorPanel.SetActive(false);
    }

    // 창 닫기 (X 버튼용)
    public void CloseDocuments() => documentsPanel.SetActive(false);
    public void CloseEditor() => editorPanel.SetActive(false);
    public void CloseDocumentView() => documentViewPanel.SetActive(false);
}
