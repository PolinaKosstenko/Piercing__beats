using UnityEngine;
using System.Collections.Generic;

public class CreateSphere : MonoBehaviour
{
    public float sphereScale = 1f;
    public Color sphereColor = Color.red;
    public AudioClip audioClip;
    public int difficulty = 3;
    public int[] availableNoteValues = { 1, 2, 4, 8 }; // Доступные длительности нот
    
    public GameObject sphereObject;
    
    private AudioSource audioSource;
    private float beatTimer;
    private float beatDuration; // Длительность четвертной ноты (1/4)
    private float currentBPM;
    private float trackDuration; // Длительность трека в секундах
    private bool isBpmAnalyzed = false;
    private bool isMusicPlaying = false;
    private Collider[] bodyColliders;
    private Transform bodyCollidersParent;
    
    private List<GameObject> activeSpheres = new List<GameObject>();
    
    private int[] speed; // Массив длительностей нот
    private float[] noteDurations; // Массив фактических длительностей в секундах
    private float totalGeneratedDuration = 0f; // Общая длительность сгенерированных нот
    private float timeSinceLastSphere = 0f;
    private int currentSpeedIndex = 0;
    private float nextSphereTime = 0f;
    
    void Start()
    {
        FindBodyCollidersInThisObject();
        SetupAudioSource();
        AnalyzeBPMAndDuration();
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
        if (bodyColliders == null || bodyColliders.Length == 0)
        {
            Debug.LogWarning("No body colliders found, spawning at default position");
            CreateSphereAtPosition(transform.position);
            return;
        }
        
        int randomIndex = Random.Range(0, bodyColliders.Length);
        Collider selectedCollider = bodyColliders[randomIndex];
        
        if (selectedCollider == null)
        {
            Debug.LogWarning("Selected collider is null, choosing another one");
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
        
        Vector3 spawnPosition = GetRandomPositionInCollider(selectedCollider);
        CreateSphereAtPosition(spawnPosition);
        
        if (currentSpeedIndex < speed.Length)
        {
            string noteName = GetNoteName(speed[currentSpeedIndex]);
            Debug.Log($"Sphere {currentSpeedIndex + 1}/{speed.Length}: {noteName} note ({noteDurations[currentSpeedIndex]:F2}s) at {selectedCollider.name}");
        }
    }
    
    string GetNoteName(int speedValue)
    {
        switch (speedValue)
        {
            case 1: return "целая";
            case 2: return "половинная";
            case 4: return "четвертная";
            case 8: return "восьмая";
            case 16: return "шестнадцатая";
            case 32: return "тридцатьвторая";
            default: return $"{speedValue}";
        }
    }
    
    void CreateSphereAtPosition(Vector3 position)
    {
        GameObject newSphere = Instantiate(sphereObject, position, Quaternion.identity);
        newSphere.name = "PulseSphere";
        
        SphereController sphereController = newSphere.AddComponent<SphereController>();
        float currentNoteDuration = noteDurations[currentSpeedIndex];
        sphereController.Initialize(sphereScale, currentNoteDuration, difficulty, sphereColor);
        
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
            trackDuration = audioClip.length;
            Debug.Log($"Track duration: {trackDuration:F2} seconds");
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
                
                ClearAllSpheres();
                
                // Показываем статистику
                Debug.Log($"=== Playback Complete ===");
                Debug.Log($"Total spheres: {speed.Length}");
                Debug.Log($"Theoretical duration: {totalGeneratedDuration:F2}s");
                Debug.Log($"Actual track duration: {trackDuration:F2}s");
                Debug.Log($"Difference: {Mathf.Abs(totalGeneratedDuration - trackDuration):F2}s");
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
    
    void AnalyzeBPMAndDuration()
    {
        if (audioClip != null)
        {
            currentBPM = UniBpmAnalyzer.AnalyzeBpm(audioClip);
            
            if (currentBPM > 0)
            {
                CalculateBeatDuration();
                isBpmAnalyzed = true;
                
                // Генерируем массив длительностей на всю длину трека
                GenerateNoteSequenceForTrack();
                
                // Вычисляем фактические длительности
                CalculateNoteDurations();
                
                // Создаем первую сферу
                CreateNewSphere();
                
                // Рассчитываем время следующей сферы
                CalculateNextSphereTime();
                
                LogGeneratedSequence();
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
        Debug.Log($"BPM: {currentBPM}, Quarter note: {beatDuration:F3}s");
    }
    
    void GenerateNoteSequenceForTrack()
    {
        List<int> noteSequence = new List<int>();
        totalGeneratedDuration = 0f;
        
        // Минимальная длительность ноты (шестнадцатая)
        float minNoteDuration = (4f / 16f) * beatDuration;
        
        // Генерируем ноты пока не заполним всю длительность трека
        while (totalGeneratedDuration < trackDuration - minNoteDuration)
        {
            // Выбираем случайную длительность ноты из доступных
            int randomNoteValue = availableNoteValues[Random.Range(0, availableNoteValues.Length)];
            
            // Рассчитываем длительность этой ноты в секундах
            float noteDuration = (4f / randomNoteValue) * beatDuration;
            
            // Проверяем, не превысим ли общую длительность
            if (totalGeneratedDuration + noteDuration <= trackDuration + minNoteDuration)
            {
                noteSequence.Add(randomNoteValue);
                totalGeneratedDuration += noteDuration;
            }
            else
            {
                // Если не помещается, попробуем более короткую ноту
                int shorterNote = FindShorterNote(randomNoteValue);
                if (shorterNote > 0)
                {
                    float shorterDuration = (4f / shorterNote) * beatDuration;
                    if (totalGeneratedDuration + shorterDuration <= trackDuration + minNoteDuration)
                    {
                        noteSequence.Add(shorterNote);
                        totalGeneratedDuration += shorterDuration;
                    }
                }
            }
        }
        
        // Конвертируем список в массив
        speed = noteSequence.ToArray();
        
        Debug.Log($"Generated {speed.Length} notes, total duration: {totalGeneratedDuration:F2}s (track: {trackDuration:F2}s)");
    }
    
    int FindShorterNote(int currentNote)
    {
        // Ищем более короткую ноту из доступных
        for (int i = 0; i < availableNoteValues.Length; i++)
        {
            if (availableNoteValues[i] > currentNote) // Большее значение = более короткая нота
            {
                return availableNoteValues[i];
            }
        }
        return -1; // Не нашли более короткую ноту
    }
    
    void CalculateNoteDurations()
    {
        noteDurations = new float[speed.Length];
        
        for (int i = 0; i < speed.Length; i++)
        {
            noteDurations[i] = (4f / speed[i]) * beatDuration;
        }
    }
    
    void CalculateNextSphereTime()
    {
        if (currentSpeedIndex < noteDurations.Length)
        {
            nextSphereTime = noteDurations[currentSpeedIndex];
        }
    }
    
    void LogGeneratedSequence()
    {
        Debug.Log("=== Generated Note Sequence ===");
        float cumulativeTime = 0f;
        
        for (int i = 0; i < speed.Length; i++)
        {
            cumulativeTime += noteDurations[i];
            string noteName = GetNoteName(speed[i]);
            string timing = $"{(cumulativeTime / 60f):F0}:{(cumulativeTime % 60f):00.0}";
            Debug.Log($"{i + 1:000}. {noteName} ({speed[i]}) - {noteDurations[i]:F3}s @ {timing}");
        }
        
        Debug.Log($"Total: {speed.Length} notes, {cumulativeTime:F2}s");
        Debug.Log("===============================");
    }

    void Update()
    {
        CheckMusicStatus();
        
        if (!isBpmAnalyzed || !isMusicPlaying || speed == null) return;

        timeSinceLastSphere += Time.deltaTime;
        
        // Очищаем неактивные сферы
        CleanupInactiveSpheres();
        
        // Показываем прогресс
        ShowProgress();
        
        // Проверяем, пора ли создать новую сферу
        if (timeSinceLastSphere >= nextSphereTime && currentSpeedIndex < speed.Length - 1)
        {
            currentSpeedIndex++;
            CreateNewSphere();
            timeSinceLastSphere = 0f;
            CalculateNextSphereTime();
        }
    }
    
    void CleanupInactiveSpheres()
    {
        for (int i = activeSpheres.Count - 1; i >= 0; i--)
        {
            if (activeSpheres[i] == null)
            {
                activeSpheres.RemoveAt(i);
            }
        }
    }
    
    void ShowProgress()
    {
        // Можно выводить прогресс в UI
        float currentTime = audioSource.time;
        float progressPercent = (currentTime / trackDuration) * 100f;
        int spheresProgress = speed.Length > 0 ? (int)((currentSpeedIndex / (float)speed.Length) * 100f) : 0;
        
        // Выводим в консоль каждые 10 секунд
        if (Mathf.FloorToInt(currentTime) % 10 == 0 && Mathf.FloorToInt(currentTime) != 0)
        {
            if (Mathf.FloorToInt(currentTime) % 10 == 0)
            {
                Debug.Log($"Progress: {currentTime:F0}/{trackDuration:F0}s ({progressPercent:F1}%), " +
                         $"Spheres: {currentSpeedIndex}/{speed.Length} ({spheresProgress}%)");
            }
        }
    }
    
    void OnDestroy()
    {
        ClearAllSpheres();
    }
}