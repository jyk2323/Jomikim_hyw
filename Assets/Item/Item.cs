using UnityEngine;
using System.Collections.Generic;

// 바닥에 놓인 아이템: 근처에 가면 "E" 안내가 뜨고, E키를 누르면 획득
public class Item : MonoBehaviour
{
    public string itemName;
    public Sprite itemSprite;
    [Tooltip("주우면 몇 개 들어오는지 (EMP 2번 쓰기 → 2)")]
    public int amount = 1;

    [Header("근처에 가면 뜨는 안내 (예: E키 그림/글자)")]
    public GameObject keyHint;

    private bool playerNearby = false;

    // 이미 주운 아이템 기억 (방을 나갔다 와도 다시 생기지 않게)
    private static HashSet<string> pickedItems = new HashSet<string>();
    string PickKey => gameObject.scene.name + "/" + itemName;

    void Start()
    {
        if (keyHint != null) keyHint.SetActive(false);

        // 이미 주운 아이템이면 없애기
        if (pickedItems.Contains(PickKey))
            Destroy(gameObject);
    }

    void Update()
    {
        // 대사 중에는 줍기 불가
        if (DialogueManager.instance != null && DialogueManager.instance.IsDialogueActive) return;

        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickupItem();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerNearby = true;
        if (keyHint != null) keyHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerNearby = false;
        if (keyHint != null) keyHint.SetActive(false);
    }

    void PickupItem()
    {
        pickedItems.Add(PickKey);
        Inventory.instance.AddItem(itemName, itemSprite, amount);
        Destroy(gameObject);
    }
}