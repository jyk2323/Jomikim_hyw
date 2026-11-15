using UnityEngine;

// 위에서 내려다보는 게임용 앞뒤 정렬:
// 화면에서 "발"이 더 아래에 있는 쪽이 앞에 그려짐 (카린, 레인 등 움직이는 캐릭터에 붙이기)
// ※ 서로 앞뒤가 바뀌어야 하는 것들은 반드시 "같은 Sorting Layer"에 있어야 함
public class SortingOrder : MonoBehaviour
{
    [Tooltip("자식 그림(그림자 등)을 본체보다 이만큼 뒤에 그림")]
    public int childOffset = -1;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer[] children;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 자식 그림들 (그림자 등) - 본체는 빼고
        SpriteRenderer[] all = GetComponentsInChildren<SpriteRenderer>(true);
        children = System.Array.FindAll(all, r => r != spriteRenderer);
    }

    void LateUpdate()
    {
        if (spriteRenderer == null) return;

        // 그림의 맨 아래(발) 높이 기준. 그림 크기나 중심점이 달라도 발 위치로 비교함
        float feetY = spriteRenderer.bounds.min.y;
        int order = Mathf.RoundToInt(-feetY * 100f);

        spriteRenderer.sortingOrder = order;

        // 그림자 등 자식은 본체와 같은 레이어, 본체 바로 뒤에
        foreach (SpriteRenderer r in children)
        {
            if (r == null) continue;
            r.sortingLayerID = spriteRenderer.sortingLayerID;
            r.sortingOrder = order + childOffset;
        }
    }
}