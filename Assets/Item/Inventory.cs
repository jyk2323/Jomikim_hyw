using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public GameObject inventoryPanel;
    public GameObject itemSlotPrefab;  // ItemSlot 프리팹
    public Transform slotParent;       // InventoryPanel

    [Header("아이템 획득 안내창")]
    public GameObject noticePanel;        // 안내창 패널
    public TextMeshProUGUI noticeText;    // 안내 문구
    public float noticeDuration = 2f;     // 화면에 떠 있는 시간(초)
    public float fadeTime = 0.25f;        // 나타나고 사라지는 시간(초)

    // 가진 아이템과 개수 (예: EMP → 2)
    private Dictionary<string, int> counts = new Dictionary<string, int>();

    // 아이템 이름 → 인벤토리 칸 (사용/삭제용)
    private Dictionary<string, GameObject> slots = new Dictionary<string, GameObject>();

    // 지금 "사용하기" 상태인 칸
    private string selectedItem = null;

    [Header("아이템 사용")]
    public string useButtonText = "사용하기";
    public string cannotUseText = "여기서는 사용할 수 없어요";

    [Header("EMP (쓰면 로봇 정지)")]
    public string empItemName = "EMP";          // 아이템 이름과 글자까지 같아야 함
    public float empDuration = 5f;              // 멈추는 시간(초)
    public bool consumeEmp = true;              // 쓰면 사라짐

    [Header("회복약 (쓰면 하트 회복)")]
    public string healItemName = "회복약";       // 아이템 이름과 글자까지 같아야 함
    public int healHearts = 3;                  // 회복하는 하트 개수
    public string fullHpMessage = "체력이 이미 가득 찼어요";

    private CanvasGroup noticeGroup;
    private Coroutine noticeRoutine;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        inventoryPanel.SetActive(false);

        if (noticePanel != null)
        {
            // 투명도 조절용 CanvasGroup (없으면 자동으로 붙임)
            noticeGroup = noticePanel.GetComponent<CanvasGroup>();
            if (noticeGroup == null)
                noticeGroup = noticePanel.AddComponent<CanvasGroup>();

            noticeGroup.blocksRaycasts = false; // 안내창이 클릭을 막지 않게
            noticePanel.SetActive(false);
        }
    }

    public void ToggleInventory()
    {
        bool isOpen = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isOpen);
        ClearSelection(); // 열거나 닫을 때 "사용하기" 상태 풀기
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        ClearSelection();
    }

    // 아이템 획득 (Item.cs의 E키 줍기, 상자, 대사 후 자동획득 모두 여기로 옴)
    // amount = 몇 개 얻는지 (EMP처럼 여러 번 쓰는 아이템은 2 등)
    public void AddItem(string itemName, Sprite itemSprite, int amount = 1)
    {
        if (amount < 1) amount = 1;

        if (counts.ContainsKey(itemName))
        {
            // 이미 있는 아이템이면 칸은 그대로, 개수만 늘림
            counts[itemName] += amount;
        }
        else
        {
            counts[itemName] = amount;

            GameObject slot = Instantiate(itemSlotPrefab, slotParent);
            slot.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;

            // 칸을 누를 수 있게 버튼으로
            Button btn = slot.GetComponent<Button>();
            if (btn == null) btn = slot.AddComponent<Button>();
            btn.onClick.AddListener(() => OnSlotClicked(itemName));
            slots[itemName] = slot;
        }

        UpdateSlotText(itemName);

        string countText = amount > 1 ? " " + amount + "개" : "";
        ShowMessage(itemName + countText + AddParticle(itemName + countText) + " 획득했어요");
    }

    // 아이템 하나 쓰기 (0개가 되면 칸도 사라짐)
    public void RemoveItem(string itemName, int amount = 1)
    {
        if (!counts.ContainsKey(itemName)) return;

        counts[itemName] -= amount;
        if (counts[itemName] > 0)
        {
            UpdateSlotText(itemName);
            return;
        }

        if (selectedItem == itemName) selectedItem = null;
        counts.Remove(itemName);

        if (slots.TryGetValue(itemName, out GameObject slot))
        {
            Destroy(slot);
            slots.Remove(itemName);
        }
    }

    // 몇 개 가지고 있나
    public int GetCount(string itemName)
    {
        return counts.TryGetValue(itemName, out int c) ? c : 0;
    }

    // 칸 글자: 1개면 이름만, 2개 이상이면 "이름 x2"
    void UpdateSlotText(string itemName)
    {
        TextMeshProUGUI txt = GetSlotText(itemName);
        if (txt == null) return;

        int c = GetCount(itemName);
        txt.text = c > 1 ? itemName + " x" + c : itemName;
    }

    // ───────── 아이템 사용 ─────────
    // 1번 클릭: 칸이 "사용하기"로 바뀜 / 2번 클릭: 사용 시도
    void OnSlotClicked(string itemName)
    {
        if (selectedItem == itemName)
        {
            UseItem(itemName);
            return;
        }

        ClearSelection();
        selectedItem = itemName;

        TextMeshProUGUI txt = GetSlotText(itemName);
        if (txt != null) txt.text = useButtonText;
    }

    void UseItem(string itemName)
    {
        ClearSelection();

        // EMP: 어디서든 사용 가능 → 로봇 정지
        if (itemName == empItemName)
        {
            RobotStop.StopFor(empDuration);
            if (consumeEmp) RemoveItem(itemName);
            CloseInventory();

            int left = GetCount(itemName);
            ShowMessage("로봇이 " + empDuration + "초 동안 멈췄어요 (남은 " + itemName + ": " + left + "개)");
            return;
        }

        // 회복약: 어디서든 사용 가능 → 하트 회복 (꽉 차 있으면 안 쓰고 남김)
        if (itemName == healItemName)
        {
            if (PlayerHealth.instance == null || !PlayerHealth.instance.HealHearts(healHearts))
            {
                ShowMessage(fullHpMessage);
                return;
            }

            RemoveItem(itemName);
            CloseInventory();
            ShowMessage("하트 " + healHearts + "개 회복!");
            return;
        }

        // 근처에 이 아이템을 쓸 수 있는 곳(문 등)이 있으면 사용
        if (LockedDoor.TryUseItem(itemName))
            return;

        ShowMessage(cannotUseText);
    }

    void ClearSelection()
    {
        string prev = selectedItem;
        selectedItem = null;
        if (prev != null) UpdateSlotText(prev);   // "사용하기" → 원래 이름(x개)으로
    }

    TextMeshProUGUI GetSlotText(string itemName)
    {
        if (!slots.TryGetValue(itemName, out GameObject slot) || slot == null) return null;
        Transform t = slot.transform.Find("ItemName");
        return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
    }

    // 이 아이템을 가지고 있나? (예: Inventory.instance.HasItem("간부실 열쇠"))
    public bool HasItem(string itemName)
    {
        return GetCount(itemName) > 0;
    }

    // ───────── 안내창 (획득, 사용 불가 등) ─────────

    public void ShowMessage(string message)
    {
        if (noticePanel == null) return; // 안내창을 연결 안 했으면 그냥 넘어감

        // 안내창이 떠 있는 중에 또 획득하면 처음부터 다시 표시
        if (noticeRoutine != null)
            StopCoroutine(noticeRoutine);

        noticeRoutine = StartCoroutine(NoticeRoutine(message));
    }

    IEnumerator NoticeRoutine(string message)
    {
        if (noticeText != null)
            noticeText.text = message;

        noticePanel.SetActive(true);

        // 나타나기
        yield return Fade(0f, 1f);

        // 잠시 유지
        yield return new WaitForSeconds(noticeDuration);

        // 사라지기
        yield return Fade(1f, 0f);

        noticePanel.SetActive(false);
        noticeRoutine = null;
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            noticeGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        noticeGroup.alpha = to;
    }

    // 마지막 글자에 받침이 있으면 "을", 없으면 "를" (한글이 아니면 "을(를)")
    string AddParticle(string word)
    {
        if (string.IsNullOrEmpty(word)) return "을(를)";

        char last = word[word.Length - 1];
        if (last < 0xAC00 || last > 0xD7A3) return "을(를)";

        bool hasBatchim = (last - 0xAC00) % 28 != 0;
        return hasBatchim ? "을" : "를";
    }
}