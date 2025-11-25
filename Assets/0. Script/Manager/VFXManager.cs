using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] ParticleSystem clickVFX;

    public void PlayClickEffect(Vector3 pos)
    {
        ParticleSystem vfx = Instantiate(clickVFX, pos, Quaternion.identity);
        vfx.Play();
        Destroy(vfx.gameObject, vfx.main.duration);
    }
}
