using UnityEngine;
using System.Collections;

public class BossIntro : MonoBehaviour
{
    public string speakerName;
    public Sprite speakerImage;
    public Sprite panelSprite;

    [TextArea]
    public string[] introLines;

    void Start()
    {
        StartCoroutine(WaitAndStart());
    }

    IEnumerator WaitAndStart()
    {
        // DialogueManager 생길 때까지 대기!
        float timeout = 5f; // 5초 넘으면 그냥 에러창으로!
        float elapsed = 0f;

        while (DialogueManager.instance == null)
        {
            elapsed += Time.deltaTime;
            if (elapsed > timeout)
            {
                // 타임아웃되면 대사 없이 바로 에러창!
                ErrorSequence.instance.StartErrors();
                yield break;
            }
            yield return null;
        }

        // 대사 있으면 출력, 없으면 바로 에러창!
        if (introLines != null && introLines.Length > 0)
        {
            DialogueManager.instance.OnDialogueEnd = StartErrorSequence;
            DialogueManager.instance.StartDialogue(
                speakerName, speakerImage, introLines, true, panelSprite, null);
        }
        else
        {
            StartErrorSequence();
        }
    }

    void StartErrorSequence()
    {
        if (ErrorSequence.instance != null)
            ErrorSequence.instance.StartErrors();
    }
}