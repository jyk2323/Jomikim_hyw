#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RemoveMissing : MonoBehaviour
{
    [MenuItem("Tools/Remove Missing Scripts")]
    static void RemoveMissingScripts()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            SerializedObject so = new SerializedObject(go);
            SerializedProperty prop = so.FindProperty("m_Component");

            for (int i = prop.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty comp = prop.GetArrayElementAtIndex(i);
                if (comp.FindPropertyRelative("component").objectReferenceValue == null)
                {
                    prop.DeleteArrayElementAtIndex(i);
                    count++;
                }
            }
            so.ApplyModifiedProperties();
        }

        Debug.Log($"Missing Scripts 제거 완료: {count}개 제거됨");
    }
}
#endif