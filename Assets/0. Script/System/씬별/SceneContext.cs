using Unity.Cinemachine;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
    
    public Player player;
    [Header("Player/TutorialCollider")]
    public TargetTrackerEmitter2D targetTrackerEmitter2D;
    
    [Header("CameraManager/Cameras/MouseVFXCamera")]
    public Camera VFXCamera;
    public CinemachineCamera cinemachineCamera;
    
    public DialoguePanel dialoguePanel;
    public LevelUpPanel levelUpPanel;
    public GameObject haveSoulsPanel;
    [Header("Project/1. Prefab/MouseClickVFX")]
    public ParticleSystem clickVFX;
}
