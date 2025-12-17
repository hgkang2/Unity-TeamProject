using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSound : MonoBehaviour
{
    AudioSource source;
    PlayerMove move;
    [SerializeField] AudioClip walkClip;

    [SerializeField] AudioClip jumpEndClip;

    [Header("Distance per step")]
    [SerializeField] float stepDistance = 5; // 값 줄이면 더 자주 남

    Vector3 lastPos;
    float accum;

    void Awake()
    {
        move = GetComponent<PlayerMove>();
        source = GetComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = false;

        lastPos = transform.position;
    }

    void Update()
    {
        if (TimeManager.IsPaused) return;

        Vector3 curPos = transform.position;
        float moved = (curPos - lastPos).magnitude;
        lastPos = curPos;

        if (!move.IsWalking)
        {
            accum = 0f;
            return;
        }

        accum += moved;
        if (accum >= stepDistance)
        {
            accum -= stepDistance;
            source.PlayOneShot(walkClip);
        }
    }

    // 점프착지 Animation 1프레임에서 실행
    public void PlayJumpEndSound()
    {
        source.PlayOneShot(jumpEndClip);
    }
}