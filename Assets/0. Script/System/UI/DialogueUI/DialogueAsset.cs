using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Game/Dialogue Asset", fileName = "DA_NewDialogue")]
public class DialogueAsset : ScriptableObject
{
    [TextArea(2, 6)]
    public string[] lines;

    // 인스펙터에서만 간단히 붙이고 싶을 때 옵션
    public UnityEvent onFinished;
}
