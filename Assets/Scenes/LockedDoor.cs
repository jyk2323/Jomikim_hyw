using UnityEngine;
using System.Collections.Generic;

// 잠긴 문: 열쇠를 쓰기 전에는 닿아도 안 넘어감.
// 문 근처에서 인벤토리의 열쇠를 "사용하기" → 문이 열리고 다음 씬으로.
// 한 번 열면 그 뒤로는 일반 문처럼 닿으면 바로 넘어감. (SceneChange 대신 붙임)
public class LockedDoor : MonoBehaviour
{
    [Header("이동할 씬")]
    public string nextSceneName = "2F_Ceo_room";
    public Vector2 spawnPoint;

    [Header("잠금")]
    public string requiredItem = "간부실 키";  // 인벤토리 아이템 이름과 글자까지 같아야 함
    public float useRange = 1.5f;              // 이 거리 안이면 "문 근처"
    public bool consumeItem = true;            // 쓰면 열쇠가 인벤토리에서 사라짐

    [Header("대사 (줄 맨 앞에 [이름])")]
    [TextArea] public string[] lockedLines;    // 잠긴 문에 닿았을 때
    [TextArea] public string[] unlockLines;    // 열쇠로 열었을 때

    // 열린 문 기억 (씬을 나갔다 와도 유지)
    private static HashSet<string> unlockedDoors = new HashSet<string>();
    // 지금 씬에 있는 잠긴 문들
    private static List<LockedDoor> doors = new List<LockedDoor>();

    private bool isChanging = false;

    string DoorId => gameObject.scene.name + "/" + nextSceneName;
    bool IsUnlocked => unlockedDoors.Contains(DoorId);

    void OnEnable() { doors.Add(this); }
    void OnDisable() { doors.Remove(this); }

    // ───────── 문에 닿았을 때 ─────────
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (IsUnlocked)
        {
            GoNextScene();
            return;
        }

        // 잠겨 있음
        if (lockedLines != null && lockedLines.Length > 0 &&
            DialogueManager.instance != null && !DialogueManager.instance.IsDialogueActive)
        {
            DialogueManager.instance.StartDialogue(lockedLines, true, null, null);
        }
    }

    // ───────── 인벤토리에서 "사용하기"를 누르면 Inventory가 부름 ─────────
    // 근처에 이 아이템으로 열 수 있는 문이 있으면 열고 true
    public static bool TryUseItem(string itemName)
    {
        if (PlayerHealth.instance == null) return false;
        Vector2 playerPos = PlayerHealth.instance.transform.position;

        foreach (LockedDoor door in doors)
        {
            if (door.IsUnlocked) continue;
            if (door.requiredItem != itemName) continue;
            if (Vector2.Distance(playerPos, door.transform.position) > door.useRange) continue;

            door.Unlock();
            return true;
        }
        return false;
    }

    void Unlock()
    {
        unlockedDoors.Add(DoorId);

        if (consumeItem) Inventory.instance.RemoveItem(requiredItem);
        Inventory.instance.CloseInventory();

        // 열리는 대사가 있으면 대사 → 끝나면 이동, 없으면 바로 이동
        if (unlockLines != null && unlockLines.Length > 0 && DialogueManager.instance != null)
        {
            DialogueManager.instance.OnDialogueEnd = GoNextScene;
            DialogueManager.instance.StartDialogue(unlockLines, true, null, null);
        }
        else
        {
            GoNextScene();
        }
    }

    void GoNextScene()
    {
        if (isChanging) return;
        isChanging = true;

        if (FadeManager.instance != null)
            FadeManager.instance.LoadScene(nextSceneName, spawnPoint);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    // Scene 화면에서 "문 근처" 범위를 원으로 보여줌
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, useRange);
    }
}