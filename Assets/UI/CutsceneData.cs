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
    }

    public Cut[] cuts;
}