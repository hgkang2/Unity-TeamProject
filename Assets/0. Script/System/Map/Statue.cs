using UnityEngine;

public class Statue : MonoBehaviour, IInteractable
{
    [Header("Drop Settings")]
    [SerializeField] GameObject[] beadPrefabs; 
    [SerializeField] Transform dropPoint;      
    [SerializeField] float dropForce = 5f;     

    [Header("Detection")]
    [SerializeField] float interactRange = 2f; // 플레이어와의 거리 체크
    private bool hasInteracted = false;
    private bool isPlayerNearby = false; // 플레이어가 근처에 있는가?

    [Header("UI Position")]
    [SerializeField] Transform interactionUIPos;
    public Transform InteractionUIPosition => interactionUIPos;

    void Update()
    {
        // 1. 이미 상호작용했으면 무시
        if (hasInteracted) return;

        // 2. 플레이어가 근처에 있고 + F키를 눌렀을 때 실행
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            DoInteract();
        }
    }

    // 외부(Player/Interactor)에서 호출할 때를 위한 함수
    public void Interact(Player player, Interactor interactor)
    {
        DoInteract();
    }

    private void DoInteract()
    {
        if (hasInteracted) return;
        
        Debug.Log("F키로 석상 상호작용 성공!");
        hasInteracted = true;
        DropBead();
    }

    private void DropBead()
    {
        if (beadPrefabs == null || beadPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, beadPrefabs.Length);
        GameObject selectedBead = Instantiate(beadPrefabs[randomIndex], dropPoint.position, Quaternion.identity);

        Rigidbody2D rb = selectedBead.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 launchDir = new Vector2(Random.Range(-0.2f, 0.2f), 1f).normalized;
            rb.AddForce(launchDir * dropForce, ForceMode2D.Impulse);
        }
    }

    // 플레이어가 근처에 있는지 체크 (Trigger 콜라이더 필요)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = false;
    }

    // IInteractable 필수 구현 (병아리 시스템 호환용)
    public bool CanInteract() => !hasInteracted;
    public bool IsAvailable() => !hasInteracted;
    public void OnFocus() { isPlayerNearby = true; }
    public void OnUnfocus() { isPlayerNearby = false; }
    public void Exit() { }
}