using UnityEngine;

public class OneTimeInteract : MonoBehaviour
{
    public string speakerName;
    public Sprite speakerImage;
    public Sprite panelSprite;

    [TextArea]
    public string[] lines;

    private bool hasInteracted = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (hasInteracted) return;

        hasInteracted = true;
        // null 추가!
        DialogueManager.instance.StartDialogue(speakerName, speakerImage, lines, true, panelSprite, null);
    }
}