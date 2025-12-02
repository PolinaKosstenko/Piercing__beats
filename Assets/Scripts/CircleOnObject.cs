using UnityEngine;
using System.Collections.Generic;

public class CreateCircle : MonoBehaviour
{
    public Sprite circleSprite;
    public float circleScale = 0.2f;
    public Color circleColor = Color.red;
    public AudioClip audioClip;
    public int[] availableNoteValues = { 1, 2, 4, 8 }; // Доступные длительности нот
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private GameObject circleObject;
    private float beatTimer;
    private float beatDuration; // Длительность четвертной ноты (1/4)
    private float currentBPM;
    private float trackDuration; // Длительность трека в секундах
    private int difficulty = 2;
    private int[] speed; // Будет заполнен автоматически
    private bool isBpmAnalyzed = false;
    private bool isCircleActive = false;
    private bool isMusicPlaying = false;
    private List<GameObject> activeCircles = new List<GameObject>();
    private float timeSinceLastCircle = 0f;
    private int currentSpeedIndex = 0;
    private float nextCircleTime = 0f;
    private float[] noteDurations; // Массив фактических длительностей в секундах
    private float totalGeneratedDuration = 0f; // Общая длительность сгенерированных нот

    void Start()
    {
        SetupAudioSource();
        AnalyzeBPMAndDuration();
    }

    void CreateNewCircle()
    {
        // Уничтожаем предыдущий круг, если есть
        if (circleObject != null && isCircleActive)
        {
            Destroy(circleObject);
        }

        circleObject = new GameObject($"FilledCircle_{Time.time}");
        circleObject.transform.SetParent(transform);
        circleObject.transform.localPosition = new Vector3(0, 0.51f, 0);
        circleObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        circleObject.transform.localScale = Vector3.one * circleScale;
        
        spriteRenderer = circleObject.AddComponent<SpriteRenderer>();
        
        if (circleSprite != null)
        {
            spriteRenderer.sprite = circleSprite;
        }
        
        spriteRenderer.color = circleColor;
        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        activeCircles.Add(circleObject);
        isCircleActive = true;
        beatTimer = 0f;
        
        // Логируем создание круга
        if (currentSpeedIndex < speed.Length)
        {
            string noteName = GetNoteName(speed[currentSpeedIndex]);
            Debug.Log($"Circle {currentSpeedIndex + 1}/{speed.Length}: {noteName} note ({noteDurations[currentSpeedIndex]:F2}s)");
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

    void AnalyzeBPMAndDuration()
    {
        if (audioClip != null)
        {
            // Получаем BPM
            currentBPM = UniBpmAnalyzer.AnalyzeBpm(audioClip);
            
            if (currentBPM > 0)
            {
                CalculateBeatDuration();
                isBpmAnalyzed = true;
                
                // Генерируем массив длительностей на всю длину трека
                GenerateNoteSequenceForTrack();
                
                // Вычисляем фактические длительности
                CalculateNoteDurations();
                
                // Создаем первый круг
                CreateNewCircle();
                
                // Рассчитываем время следующего круга
                CalculateNextCircleTime();
                
                LogGeneratedSequence();
            }
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

    void CalculateNextCircleTime()
    {
        if (currentSpeedIndex < noteDurations.Length)
        {
            nextCircleTime = noteDurations[currentSpeedIndex];
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

        timeSinceLastCircle += Time.deltaTime;
        beatTimer += Time.deltaTime;
        
        // Проверяем, пора ли создать новый круг
        if (timeSinceLastCircle >= nextCircleTime && currentSpeedIndex < speed.Length - 1)
        {
            currentSpeedIndex++;
            CreateNewCircle();
            timeSinceLastCircle = 0f;
            CalculateNextCircleTime();
        }
        
        // Обновляем анимацию для активного круга
        if (isCircleActive && circleObject != null)
        {
            UpdateCircleAnimation();
        }
        
        // Очищаем неактивные круги
        CleanupInactiveCircles();
        
        // Показываем прогресс
        ShowProgress();
    }

    void UpdateCircleAnimation()
    {
        if (currentSpeedIndex > 0 && currentSpeedIndex <= noteDurations.Length)
        {
            int prevSpeedIndex = currentSpeedIndex - 1;
            float currentNoteDuration = noteDurations[prevSpeedIndex];
            
            float animationProgress = Mathf.Clamp01(timeSinceLastCircle / currentNoteDuration);
            float scaleProgress = 1f - animationProgress;
            
            // Плавное уменьшение размера
            float currentScale = circleScale * scaleProgress;
            circleObject.transform.localScale = Vector3.one * currentScale;
            
            // Плавное исчезновение
            Color currentColor = spriteRenderer.color;
            currentColor.a = scaleProgress;
            spriteRenderer.color = currentColor;
        }
    }

    void ShowProgress()
    {
        // Можно выводить прогресс в UI
        float currentTime = audioSource.time;
        float progressPercent = (currentTime / trackDuration) * 100f;
        int notesProgress = (int)((currentSpeedIndex / (float)speed.Length) * 100f);
        
        // Выводим в консоль каждые 10 секунд
        if (Mathf.FloorToInt(currentTime) % 10 == 0 && Mathf.FloorToInt(currentTime) != 0)
        {
            if (Mathf.FloorToInt(currentTime) % 10 == 0)
            {
                Debug.Log($"Progress: {currentTime:F0}/{trackDuration:F0}s ({progressPercent:F1}%), " +
                         $"Notes: {currentSpeedIndex}/{speed.Length} ({notesProgress}%)");
            }
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
                ClearAllCircles();
                
                // Показываем статистику
                Debug.Log($"=== Playback Complete ===");
                Debug.Log($"Total notes: {speed.Length}");
                Debug.Log($"Theoretical duration: {totalGeneratedDuration:F2}s");
                Debug.Log($"Actual track duration: {trackDuration:F2}s");
                Debug.Log($"Difference: {Mathf.Abs(totalGeneratedDuration - trackDuration):F2}s");
            }
        }
    }

    void CleanupInactiveCircles()
    {
        for (int i = activeCircles.Count - 1; i >= 0; i--)
        {
            if (activeCircles[i] != null)
            {
                SpriteRenderer sr = activeCircles[i].GetComponent<SpriteRenderer>();
                if (sr != null && sr.color.a <= 0.01f)
                {
                    Destroy(activeCircles[i]);
                    activeCircles.RemoveAt(i);
                }
            }
        }
    }

    void ClearAllCircles()
    {
        foreach (var circle in activeCircles)
        {
            if (circle != null)
            {
                Destroy(circle);
            }
        }
        activeCircles.Clear();
        isCircleActive = false;
        circleObject = null;
    }

    void OnDestroy()
    {
        ClearAllCircles();
    }

    // Метод для перегенерации последовательности с теми же параметрами
    public void RegenerateSequence()
    {
        if (isBpmAnalyzed)
        {
            ClearAllCircles();
            GenerateNoteSequenceForTrack();
            CalculateNoteDurations();
            currentSpeedIndex = 0;
            timeSinceLastCircle = 0f;
            CreateNewCircle();
            CalculateNextCircleTime();
            LogGeneratedSequence();
        }
    }

    // Метод для генерации с определенной плотностью нот
    public void GenerateWithDensity(float notesPerSecond)
    {
        ClearAllCircles();
        
        // Рассчитываем примерное количество нот
        int targetNoteCount = Mathf.RoundToInt(trackDuration * notesPerSecond);
        List<int> noteSequence = new List<int>();
        totalGeneratedDuration = 0f;
        
        while (noteSequence.Count < targetNoteCount && totalGeneratedDuration < trackDuration)
        {
            int randomNoteValue = availableNoteValues[Random.Range(0, availableNoteValues.Length)];
            float noteDuration = (4f / randomNoteValue) * beatDuration;
            
            if (totalGeneratedDuration + noteDuration <= trackDuration)
            {
                noteSequence.Add(randomNoteValue);
                totalGeneratedDuration += noteDuration;
            }
        }
        
        speed = noteSequence.ToArray();
        CalculateNoteDurations();
        currentSpeedIndex = 0;
        timeSinceLastCircle = 0f;
        CreateNewCircle();
        CalculateNextCircleTime();
        
        Debug.Log($"Generated {speed.Length} notes with density {notesPerSecond}/sec");
    }
}