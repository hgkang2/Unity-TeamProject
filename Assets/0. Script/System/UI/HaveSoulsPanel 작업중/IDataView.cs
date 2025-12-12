using UnityEngine;

// 데이터를 받아서 표시할 수 있는 UI
public interface IDataView<T>
{
    RectTransform Rect { get; }
    GameObject GO { get; }

    void Bind(T data);
    void Clear();
}
