using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] ParticleSystem clickVFX;

    public void MouseClickVFX()
    {
        Vector2 clickPos = InputManager.Instance.GetMouseWorldPos();
        ParticleSystem vfx = Instantiate(clickVFX, clickPos, Quaternion.identity);
        vfx.Play();
        Destroy(vfx.gameObject, vfx.main.duration);
    }
}
