using UnityEngine;
using RuntimeInspectorNamespace;
using Unity.VisualScripting;

public class RuntimeInspectorExample : MonoBehaviour
{
    [SerializeField] RuntimeInspector inspector;
    [SerializeField] RuntimeHierarchy hierarchy;

    void Start()
    {
        // 서로 연결
        inspector.ConnectedHierarchy = hierarchy;
        hierarchy.ConnectedInspector = inspector;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // 두 UI가 같은 부모(Canvas 밑)라면 하나만 기준으로 토글해도 됨
            bool active = inspector.gameObject.activeSelf;
            inspector.gameObject.SetActive(!active);
            hierarchy.gameObject.SetActive(!active);
        }
    }
} 