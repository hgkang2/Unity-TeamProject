using UnityEngine;

public abstract class OpenStackUIPanel : MonoBehaviour, IOpenStackUI
{
    [SerializeField] bool pauseGame = true;
    public bool PauseGame => pauseGame;

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}