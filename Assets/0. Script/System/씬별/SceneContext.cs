using Unity.Cinemachine;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
    
    public Player player;
    [Header("Player/TutorialCollider")]
    
    [Header("CameraManager/Cameras/MouseVFXCamera")]
    public Camera VFXCamera;
    public CinemachineCamera cinemachineCamera;
    public CinemachineCamera cinemachineCamera_tutorialTrap;

    public DialoguePanel dialoguePanel;
    public LevelUpPanel levelUpPanel;
    public HaveSoulPanel haveSoulsPanel;
    public IngameSettingPanel ingameSettingPanel;

    [Header("Stage1 Tutorial")]
    public TutorialTrigger2D tutorialTrigger_monsterMeet;
    public Collider2D tutorialWall;
    public TutorialTrigger2D tutorialTrigger_trap;

    [Header("Project/1. Prefab/MouseClickVFX")]
    public ParticleSystem clickVFX;
}
