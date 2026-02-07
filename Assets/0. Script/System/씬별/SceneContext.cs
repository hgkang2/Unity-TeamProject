using Unity.Cinemachine;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
    
    public Player player;
    public Interactor interactor;
    [Header("Player/TutorialCollider")]
    
    [Header("CameraManager/Cameras/MouseVFXCamera")]
    public Camera VFXCamera;
    public CinemachineCamera cinemachineCamera;
    public CinemachineCamera cinemachineCamera_tutorialTrap;

    public Canvas canvas;
    public DialoguePanel dialoguePanel;
    public LevelUpPanel levelUpPanel;
    public HaveSoulPanel haveSoulsPanel;
    public IngameSettingPanel ingameSettingPanel;
    public AltarUI altarUI;

    [Header("Stage1 Tutorial")]
    public TutorialTrigger2D tutorialTrigger_monsterMeet;
    public Collider2D tutorialWall;
    public TutorialTrigger2D tutorialTrigger_trap;
    public TutorialTrigger2D tutorialTrigger_flyMonster;
    public TutorialTrigger2D tutorialTrigger_breakableGround;
    public TutorialTrigger2D tutorialTrigger_bossRoom;

    [Header("Project/1. Prefab/MouseClickVFX")]
    public ParticleSystem clickVFX;
}
