using UnityEngine;
using UnityEngine.UI;

public class MyButton : MonoBehaviour
{
    Button button;
    [SerializeField] Image activeImage;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Activate()
    {
        button.interactable = true;
        activeImage.enabled = true;
    }

    public void DeActivate()
    {
        button.interactable = false;
        activeImage.enabled = false;
    }

    public void Click()
    {
        button.onClick?.Invoke();
    }
}
