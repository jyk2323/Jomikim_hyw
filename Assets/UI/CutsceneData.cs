using UnityEngine;

[CreateAssetMenu(fileName = "NewCutscene", menuName = "Game/Cutscene Data")]
public class CutsceneData : ScriptableObject
{
    [System.Serializable]
    public class Cut
    {
        public Sprite image;
        [TextArea(2, 5)]
        public string[] lines;
        public float autoNextTime = 2f;
        [Tooltip("이 컷이 나올 때 한 번 재생할 효과음 (비우면 없음). 예: 폭발음")]
        public AudioClip sound;            // ← 추가
        [Tooltip("컷이 바뀌고 몇 초 뒤에 효과음을 재생할지")]
        public float soundDelay = 0f;      // ← 추가
    }

    public Cut[] cuts;
}