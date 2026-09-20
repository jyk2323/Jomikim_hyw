using UnityEngine;

public class BossIntro : MonoBehaviour
{
    public string speakerName;
    public Sprite speakerImage;
    public Sprite panelSprite;

    [TextArea]
    public string[] introLines; // 주인공 & 보스 대사

    void Start()
    {
        // 씬 시작하자마자 대사 시작!
        DialogueManager.instance.StartDialogue(
            speakerName, speakerImage, introLines, true, panelSprite, null);

        // 대사 끝나면 에러창 띄우기
        DialogueManager.instance.OnDialogueEnd = StartErrorSequence;
    }

    void StartErrorSequence()
    {
        ErrorSequence.instance.StartErrors();
    }
}