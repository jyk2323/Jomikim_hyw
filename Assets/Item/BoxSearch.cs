using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 상자 조사: 근처에 가면 "확인하기" 버튼 → 누르면 팝업창에 상자 속 아이템 표시
// → 아이템을 누르면 인벤토리에 획득. 다 가져가면 다음부터는 "비었다" 대사.
public class BoxSearch : MonoBehaviour
{
    [System.Serializable]
    public class BoxItem
    {
        public string itemName;    // 인벤토리에 들어갈 이름 (예: 피 묻은 쪽지)
        public Sprite itemSprite;  // 아이템 그림
    }

    [Header("상자 속 아이템")]
    public BoxItem[] items;

    [Header("UI 연결")]
    public GameObject interactCanvas;  // 근처에 가면 뜨는 확인하기 버튼 (InteractCanvas)
    public GameObject popupPanel;      // 상자 팝업창
    public Transform itemListParent;   // 팝업 안에서 아이템 칸들이 들어갈 곳
    public GameObject itemSlotPrefab;  // 아이템 칸 프리팹 (Item/ItemSlot 재사용)

    [Header("대사 (줄 맨 앞에 [이름])")]
    [TextArea] public string[] openLines;   // 팝업 열기 전에 할 대사 (비우면 바로 팝업)
    [TextArea] public string[] emptyLines;  // 다 가져간 뒤 조사했을 때 대사

    private bool playerNearby = false;
    private bool isOpen = false;
    private PlayerMove playerMove;

    void Start()
    {
        if (interactCanvas != null) interactCanvas.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void Update()
    {
        // 팝업이 열려 있을 때 Esc로 닫기
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePopup();
    }

    // ───────── 근처 감지 ─────────
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerNearby = true;
        if (!isOpen && interactCanvas != null) interactCanvas.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerNearby = false;
        if (interactCanvas != null) interactCanvas.SetActive(false);
    }

    // ───────── 확인하기 버튼 (InteractCanvas의 Button OnClick에 연결) ─────────
    public void OnInteractButtonClick()
    {
        if (isOpen) return;
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive) return;

        if (interactCanvas != null) interactCanvas.SetActive(false);

        // 남은 아이템이 없으면 "비었다" 대사만
        if (RemainingItems().Count == 0)
        {
            if (emptyLines != null && emptyLines.Length > 0 && DialogueManager.instance != null)
            {
                DialogueManager.instance.OnDialogueEnd = ShowButtonAgain;
                DialogueManager.instance.StartDialogue(emptyLines, true, null, null);
            }
            else
            {
                ShowButtonAgain();
            }
            return;
        }

        // 열기 전 대사가 있으면 대사 → 끝나면 팝업
        if (openLines != null && openLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.OnDialogueEnd = OpenPopup;
            DialogueManager.instance.StartDialogue(openLines, true, null, null);
        }
        else
        {
            OpenPopup();
        }
    }

    // ───────── 팝업 ─────────
    void OpenPopup()
    {
        isOpen = true;
        SetPlayerMove(false);

        // 아이템 칸 새로 만들기 (이미 가진 아이템은 빼고)
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);

        foreach (BoxItem item in RemainingItems())
            CreateSlot(item);

        popupPanel.SetActive(true);
    }

    // 닫기 버튼 OnClick에 연결
    public void ClosePopup()
    {
        isOpen = false;
        popupPanel.SetActive(false);
        SetPlayerMove(true);
        ShowButtonAgain();
    }

    void CreateSlot(BoxItem item)
    {
        GameObject slot = Instantiate(itemSlotPrefab, itemListParent);

        Transform img = slot.transform.Find("ItemImage");
        if (img != null) img.GetComponent<Image>().sprite = item.itemSprite;

        Transform txt = slot.transform.Find("ItemName");
        if (txt != null) txt.GetComponent<TextMeshProUGUI>().text = item.itemName;

        // 칸 자체를 버튼으로 만들어서 누르면 획득
        Button btn = slot.GetComponent<Button>();
        if (btn == null) btn = slot.AddComponent<Button>();
        btn.onClick.AddListener(() => TakeItem(item, slot));
    }

    void TakeItem(BoxItem item, GameObject slot)
    {
        if (Inventory.instance.HasItem(item.itemName)) return; // 두 번 획득 방지

        Inventory.instance.AddItem(item.itemName, item.itemSprite); // 안내창도 여기서 뜸
        Destroy(slot);

        // 마지막 아이템이었으면 팝업 자동으로 닫기
        if (RemainingItems().Count == 0)
            ClosePopup();
    }

    // 아직 인벤토리에 없는 아이템만
    // (인벤토리 기준이라 씬을 나갔다 와도 다시 생기지 않음)
    List<BoxItem> RemainingItems()
    {
        List<BoxItem> list = new List<BoxItem>();
        foreach (BoxItem item in items)
            if (!Inventory.instance.HasItem(item.itemName))
                list.Add(item);
        return list;
    }

    // ───────── 기타 ─────────
    void ShowButtonAgain()
    {
        if (playerNearby && interactCanvas != null) interactCanvas.SetActive(true);
    }

    void SetPlayerMove(bool canMove)
    {
        if (playerMove == null && PlayerHealth.instance != null)
            playerMove = PlayerHealth.instance.GetComponent<PlayerMove>();
        if (playerMove != null)
            playerMove.canMove = canMove;
    }
}