using UnityEngine;

public class AttackSpriteVFX : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 1);
    }

    //Object Pooling 대비
    // [SerializeField] Animation anim;
    // Coroutine lifeRoutine;
    // public void Play()
    // {
    //     // 재사용 대비 초기화
    //     if (lifeRoutine != null)
    //         StopCoroutine(lifeRoutine);

    //     anim.Play();
    //     lifeRoutine = StartCoroutine(LifeRoutine());
    // }

    // IEnumerator LifeRoutine()
    // {
    //     yield return new WaitForSeconds(anim.clip.length);
    //     gameObject.SetActive(false);
    // }
}
