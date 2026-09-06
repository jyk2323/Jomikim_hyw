using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    public GameObject inventoryPanel;
    public GameObject itemSlotPrefab;  // ItemSlot 프리팹
    public Transform slotParent;       // InventoryPanel

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
    }

    public void ToggleInventory()
    {
        bool isOpen = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isOpen);
    }

    public void AddItem(string itemName, Sprite itemSprite)
    {
        GameObject slot = Instantiate(itemSlotPrefab, slotParent);
        slot.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = itemName;
    }
}