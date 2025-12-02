using UnityEngine;
using System.Collections.Generic;

public class CreateSphere : MonoBehaviour
{
    public float sphereScale =  1f;
    public Color sphereColor = Color.greenYellow;
    public AudioClip audioClip;
    public int difficulty = 3;
    public int speed = 1;
    public GameObject sphereObject;
    
    private Renderer sphereRenderer;
    private AudioSource audioSource;
    private float beatTimer;
    private float beatDuration;
    private int currentBPM;
    private bool isBpmAnalyzed = false;
    private bool isSphereActive = false;
    private bool isMusicPlaying = false;
    private Collider[] bodyColliders;
    private Transform bodyCollidersParent;
    
    private List<GameObject> activeSpheres = new List<GameObject>();
    
    void Start()
    {
        FindBodyCollidersInThisObject();
        SetupAudioSource();
        AnalyzeBPM();
    }
    
    void FindBodyCollidersInThisObject()
    {
        bodyCollidersParent = FindDeepChild(transform, "Body's Colliders");
        
        if (bodyCollidersParent == null)
        {
            Debug.LogError($"Could not find 'Body's Colliders' in {gameObject.name} hierarchy!");
            return;
        }
        
        Debug.Log($"Found 'Body's Colliders' at: {bodyCollidersParent.name}");
        
        bodyColliders = new Collider[bodyCollidersParent.childCount];
        int validCollidersCount = 0;
        
        for (int i = 0; i < bodyCollidersParent.childCount; i++)
        {
            Transform child = bodyCollidersParent.GetChild(i);
            Collider collider = child.GetComponent<Collider>();
            
            if (collider != null)
            {
                bodyColliders[validCollidersCount] = collider;
                validCollidersCount++;
                Debug.Log($"Found collider: {child.name}");
            }
            else
            {
                Debug.LogWarning($"Child '{child.name}' has no Collider component!");
            }
        }
        
        if (validCollidersCount < bodyColliders.Length)
        {
            Collider[] tempArray = new Collider[validCollidersCount];
            for (int i = 0; i < validCollidersCount; i++)
            {
                tempArray[i] = bodyColliders[i];
            }
            bodyColliders = tempArray;
        }
        
        Debug.Log($"Found {bodyColliders.Length} body part colliders");
    }
    
    Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;
        
        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        
        return null;
    }
    
    Vector3 GetRandomPositionInCollider(Collider collider)
    {
        if (collider == null) return Vector3.zero;
        
        if (collider is BoxCollider boxCollider)
        {
            Vector3 center = boxCollider.center;
            Vector3 size = boxCollider.size;
            
            Vector3 localRandomPoint = new Vector3(0, Random.Range(-size.y / 2, size.y / 2), 0) + center;
            
            return boxCollider.transform.TransformPoint(localRandomPoint);
        }
        else
        {
            Bounds bounds = collider.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }
    }

    void CreateNewSphere()
    {
        // Выбираем случайный коллайдер
        int randomIndex = Random.Range(0, bodyColliders.Length);
        Collider selectedCollider = bodyColliders[randomIndex];
        
        if (selectedCollider == null)
        {
            Debug.LogWarning("Selected collider is null, choosing another one");
            // Пробуем найти другой валидный коллайдер
            foreach (Collider coll in bodyColliders)
            {
                if (coll != null)
                {
                    selectedCollider = coll;
                    break;
                }
            }
            
            if (selectedCollider == null)
            {
                CreateSphereAtPosition(transform.position);
                return;
            }
        }
        
        // Получаем случайную позицию внутри выбранного коллайдера
        Vector3 spawnPosition = GetRandomPositionInCollider(selectedCollider);
        CreateSphereAtPosition(spawnPosition);
        Debug.Log($"Spawning sphere in: {selectedCollider.name}");
    }
    
    void CreateSphereAtPosition(Vector3 position)
    {
        GameObject newSphere =  Instantiate(sphereObject, position, Quaternion.identity);
        newSphere.name = "PulseSphere";
        newSphere.transform.localScale = Vector3.one * sphereScale;
    
        sphereRenderer = newSphere.GetComponent<Renderer>();
    
        SphereController sphereController = newSphere.AddComponent<SphereController>();
        sphereController.Initialize(sphereScale, beatDuration, difficulty);
        
        activeSpheres.Add(newSphere);
    }
    
    void SetupAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.15f;

        if (audioClip != null)
        {
            audioSource.Play();
            isMusicPlaying = true;
        }
    }
    
    void CheckMusicStatus()
    {
        if (audioSource != null && isMusicPlaying)
        {
            if (!audioSource.isPlaying)
            {
                isMusicPlaying = false;
                Debug.Log("Music ended");
                
                // Очищаем все сферы
                ClearAllSpheres();
            }
        }
    }
    
    void ClearAllSpheres()
    {
        foreach (GameObject sphere in activeSpheres)
        {
            if (sphere != null)
            {
                Destroy(sphere);
            }
        }
        activeSpheres.Clear();
    }
    
    void AnalyzeBPM()
    {
        if (audioClip != null)
        {
            currentBPM = UniBpmAnalyzer.AnalyzeBpm(audioClip) / (4/speed);
            
            if (currentBPM > 0)
            {
                CalculateBeatDuration();
                isBpmAnalyzed = true;
                CreateNewSphere();
            }
            else
            {
                Debug.LogError("Failed to analyze BPM");
                isBpmAnalyzed = false;
            }
        }
        else
        {
            Debug.LogError("Audio clip is not assigned!");
            isBpmAnalyzed = false;
        }
    }
    
    void CalculateBeatDuration()
    {
        beatDuration = 60f / currentBPM;
        Debug.Log("Beat duration in seconds: " + beatDuration);
    }

    void Update()
    {
        CheckMusicStatus();
        
        if (!isBpmAnalyzed || !isMusicPlaying) return;

        beatTimer += Time.deltaTime;
        
        // float pulseProgress = (beatTimer * 2 * difficulty) / beatDuration;

        for (int i = activeSpheres.Count - 1; i >= 0; i--)
        {
            if (activeSpheres[i] == null)
            {
                activeSpheres.RemoveAt(i);
            }
        }
        
        if (beatTimer >= beatDuration)
        {
            beatTimer = 0f;
            CreateNewSphere();
        }
    }
}