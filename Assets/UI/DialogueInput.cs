using UnityEngine.InputSystem;

// 다음 대사로 넘기는 입력을 한 곳에서 관리 (Space 또는 마우스 왼쪽 클릭)
public static class DialogueInput
{
    public static bool NextPressed()
    {
        bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return space || click;
    }
}