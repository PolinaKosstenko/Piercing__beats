using UnityEngine;

public class TriggerToSphere : MonoBehaviour
{
    [Header("Настройки активации")]
    public float activationRadius = 7f;
    public float deactivationRadius = 9f; // Радиус для выключения
    
    [Header("Визуализация")]
    public bool showGizmos = true;
    public Color activationColor = Color.green;
    public Color deactivationColor = Color.yellow;
    
    private Transform player;
    private CreateSphere sphereCreator;
    private bool isActive = false;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = playerObj.transform;
        sphereCreator = GetComponent<CreateSphere>();
        sphereCreator.enabled = false;
        StopAudioOnObject();
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
        StopAudioOnObject();
    }
    
    void StopAudioOnObject()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        foreach (AudioSource audio in audioSources)
        {
            if (audio.isPlaying) audio.Stop();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Gizmos.color = activationColor;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        Gizmos.color = deactivationColor;
        Gizmos.DrawWireSphere(transform.position, deactivationRadius);
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;
        
        // В игровом режиме показываем статус цветом
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}