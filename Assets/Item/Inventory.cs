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
    }

    // 아이템 획득 (Item.cs의 E키 줍기, DialogueManager의 자동획득 둘 다 여기로 옴)
    public void AddItem(string itemName, Sprite itemSprite)
    {
        GameObject slot = Instantiate(itemSlotPrefab, slotParent);
        slot.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName;

        ShowNotice(itemName);
    }

    // ───────── 획득 안내창 ─────────

    void ShowNotice(string itemName)
    {
        if (noticePanel == null) return; // 안내창을 연결 안 했으면 그냥 넘어감

        // 안내창이 떠 있는 중에 또 획득하면 처음부터 다시 표시
        if (noticeRoutine != null)
            StopCoroutine(noticeRoutine);

        noticeRoutine = StartCoroutine(NoticeRoutine(itemName));
    }

    IEnumerator NoticeRoutine(string itemName)
    {
        if (noticeText != null)
            noticeText.text = itemName + AddParticle(itemName) + " 획득했어요";

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