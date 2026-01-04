using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public PlatformController[] topPlatforms;
    public PlatformController[] bottomPlatforms;

    void Start()
    {
       
        foreach (var p in topPlatforms)
        {
            StartCoroutine(p.StartCycle(0f, 3f));
        }
        foreach (var p in bottomPlatforms)
        {
            StartCoroutine(p.StartCycle(3.5f, 3f)); 
        }
    }
}