using UnityEngine;

public class Trigger : MonoBehaviour
{
    [Header("Настройки активации")]
    public float activationRadius = 7f;
    public float deactivationRadius = 9f;
    
    [Header("Визуализация")]
    public bool showGizmos = true;
    
    private Transform player;
    private CreateSphere sphereCreator;
    private bool isActive = false;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); 
        player = playerObj.transform;
        sphereCreator = GetComponent<CreateSphere>();
    }
    
    void Update()
    {
        if (player == null || sphereCreator == null) return;
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (!isActive && distance <= activationRadius) ActivateSpheres();
        else if (isActive && distance > deactivationRadius) DeactivateSpheres();
    }
    
    void ActivateSpheres()
    {
        isActive = true;
        sphereCreator.enabled = true;
    }
    
    void DeactivateSpheres()
    {
        isActive = false;
        sphereCreator.enabled = false;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deactivationRadius);
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;
        
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}