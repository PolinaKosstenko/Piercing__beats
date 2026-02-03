using UnityEngine;
using System.Collections.Generic;

public class CreateSphere : MonoBehaviour
{
    public float sphereScale = 1f;
    public Color sphereColor = Color.red;
    public AudioClip audioClip;
    public int difficulty = 2;
    public int[] availableNoteValues = { 1, 2, 4 }; // Доступные длительности нот

    public GameObject sphereObject;

    private AudioSource audioSource;
    private float beatDuration;
    private float currentBPM;
    private float trackDuration;
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
    private HpManager _hpManager;

    public int[] NoteSequence => speed;

    void Start()
    {
        _hpManager = GetComponent<HpManager>();
        if (_hpManager == null)
            _hpManager = GetComponentInParent<HpManager>();
        if (_hpManager == null)
            _hpManager = FindFirstObjectByType<HpManager>();

        FindBodyCollidersInThisObject();
        SetupAudioSource();
        AnalyzeBPMAndDuration();
    }
    
    void OnEnable()
    {
        if (!isMusicPlaying && audioSource != null && audioClip != null) StartMusic();
        if (!isBpmAnalyzed && audioClip != null) AnalyzeBPMAndDuration();
        
        timeSinceLastSphere = 0f;
        currentSpeedIndex = 0;
        
        if (noteDurations != null && noteDurations.Length > 0)
        {
            nextSphereTime = noteDurations[0];
        }
    }
    
    void OnDisable()
    {
        StopMusic();
        ClearAllSpheres();
    }
    
    void FindBodyCollidersInThisObject()
    {
        bodyCollidersParent = FindDeepChild(transform, "Body's Colliders");
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
            Debug.LogWarning("No colliders!");
            return;
        }

        int randomIndex = Random.Range(0, bodyColliders.Length);
        Collider selectedCollider = bodyColliders[randomIndex];

        if (selectedCollider == null)
        {
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
                return;
            }
        }

        Vector3 spawnPosition = GetRandomPositionInCollider(selectedCollider);
        
        GameObject newSphere = Instantiate(sphereObject, spawnPosition, Quaternion.identity);
        newSphere.name = "PulseSphere";

        SphereController sphereController = newSphere.AddComponent<SphereController>();
        float currentNoteDuration = noteDurations[currentSpeedIndex];

        // Передаем параметры, но скорость сжатия теперь фиксированная
        sphereController.Initialize(sphereScale, currentNoteDuration, difficulty, sphereColor);

        activeSpheres.Add(newSphere);
        
        Debug.Log($"Создана сфера #{currentSpeedIndex + 1} на {selectedCollider.name}");
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.clip = audioClip;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.15f;

        if (audioClip != null)
        {
            trackDuration = audioClip.length;
            Debug.Log($"Длительность трека: {trackDuration:F2} секунд");
        }
    }
    
    void StartMusic()
    {
        if (audioSource != null && audioClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            isMusicPlaying = true;
        }
    }
    
    void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            isMusicPlaying = false;
        }
    }

    /// <summary>
    /// Если робот мёртв — останавливает музыку и очищает сферы. Возвращает true, если музыка остановлена.
    /// </summary>
    bool StopMusicIfRobotDead()
    {
        if (_hpManager == null || _hpManager.IsAlive) return false;

        if (isMusicPlaying)
        {
            isMusicPlaying = false;
            if (audioSource != null)
                audioSource.Stop();
            ClearAllSpheres();
            Debug.Log("Robot died — music stopped.");
        }
        return true;
    }

    void CheckMusicStatus()
    {
        if (audioSource != null && isMusicPlaying)
        {
            if (!audioSource.isPlaying)
            {
                isMusicPlaying = false;
                enabled = false;
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
        if (audioClip == null) return;
        
        currentBPM = UniBpmAnalyzer.AnalyzeBpm(audioClip);
        
        if (currentBPM > 0)
        {
            beatDuration = 60f / currentBPM;
            isBpmAnalyzed = true;
            
            Debug.Log($"BPM: {currentBPM}, Длительность бита: {beatDuration:F3}с");
            
            GenerateNoteSequenceForTrack();
            CalculateNoteDurations();
            
            if (noteDurations != null && noteDurations.Length > 0)
            {
                nextSphereTime = noteDurations[0];
            }
        }
        else
        {
            Debug.LogError("Error!");
        }
    }

    void CalculateBeatDuration()
    {
        beatDuration = 60f / currentBPM;
        Debug.Log($"BPM: {currentBPM}, Quarter note: {beatDuration:F3}s");
    }

    void GenerateNoteSequenceForTrack()
    {
        if (beatDuration <= 0) return;
        
        List<int> noteSequence = new List<int>();
        totalGeneratedDuration = 0f;

        // Минимальная длительность ноты (шестнадцатая)
        float minNoteDuration = (4f / 16f) * beatDuration;

        // Генерируем ноты пока не заполним всю длительность трека
        while (totalGeneratedDuration < trackDuration - minNoteDuration)
        {
            int randomNoteValue = availableNoteValues[Random.Range(0, availableNoteValues.Length)];

            // Рассчитываем длительность этой ноты в секундах
            float noteDuration = (4f / randomNoteValue) * beatDuration * 2;

            // Проверяем, не превысим ли общую длительность
            if (totalGeneratedDuration + noteDuration <= trackDuration + minNoteDuration)
            {
                noteSequence.Add(randomNoteValue);
                totalGeneratedDuration += noteDuration;
            }
            else
            {
                // Пытаемся найти более короткую ноту
                bool foundShorter = false;
                for (int i = 0; i < availableNoteValues.Length; i++)
                {
                    if (availableNoteValues[i] > randomNoteValue)
                    {
                        float shorterDuration = (4f / availableNoteValues[i]) * beatDuration;
                        if (totalGeneratedDuration + shorterDuration <= trackDuration + minNoteDuration)
                        {
                            noteSequence.Add(availableNoteValues[i]);
                            totalGeneratedDuration += shorterDuration;
                            foundShorter = true;
                            break;
                        }
                    }
                }
                
                if (!foundShorter) break;
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
        if (speed == null) return;
        
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
        if (StopMusicIfRobotDead())
            return;

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
            CreateNewSphere();
            timeSinceLastSphere = 0f;
            currentSpeedIndex++;
            
            if (currentSpeedIndex < noteDurations.Length)
            {
                nextSphereTime = noteDurations[currentSpeedIndex];
            }
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
        StopMusic();
    }
}