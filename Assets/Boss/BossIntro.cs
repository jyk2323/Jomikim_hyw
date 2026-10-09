using UnityEngine;
using System.Collections;

public class BossIntro : MonoBehaviour
{
    public Sprite panelSprite;   // 대사창 배경 (비우면 기본)

    [TextArea]
    public string[] introLines;  // 줄마다 맨 앞에 [이름]. 예) [레인] 드디어 만났군

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
            DialogueManager.instance.StartDialogue(introLines, true, panelSprite, null);
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