using UnityEngine;

public class GrenadeTrajectory : MonoBehaviour
{
    public LineRenderer lr;
    public int segments = 30;
    float gravityScale = 1f;

    void Awake()
    {
        if(!lr) lr = GetComponent<LineRenderer>();
        if(lr) lr.enabled = false;
    }

    public void Show(Vector2 targetPos, float travelTime)
    {
        Vector2 start = transform.position;

        Vector2 lrStart = CalculateVelocity(start, targetPos, travelTime);
        Vector2 gVec = Physics2D.gravity * gravityScale;

        lr.positionCount = segments + 1;

        for(int i = 0; i <= segments; i++)
        {
            float t = (travelTime * i) / segments;
            Vector2 pos = start + lrStart * t + 0.5f * gVec * t * t;
            lr.SetPosition(i, pos);
        }

        lr.enabled = true;
    }

    public void Hide()
    {
        if(lr) lr.enabled = false;
    }

    Vector2 CalculateVelocity(Vector2 start, Vector2 target, float time)
    {
        Vector2 distance = target - start;
        float g = -Physics2D.gravity.y * gravityScale;

        float velocityX = distance.x / time;
        float velocityY = (distance.y + 1f * g * time * time) / time;

        return new Vector2(velocityX, velocityY);
    }
}
