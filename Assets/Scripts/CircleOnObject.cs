using UnityEngine;

public class CreateCircle : MonoBehaviour
{
    public Sprite circleSprite;
    public float circleScale = 0.2f;
    public Color circleColor = Color.cadetBlue;
    
    public AudioClip audioClip;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private GameObject circleObject;
    private float beatTimer;
    private float beatDuration;
    private int currentBPM;
    private bool isBpmAnalyzed = false;
    private bool isCircleActive = false;
    private bool isMusicPlaying = false;
    

    void Start()
    {
        SetupAudioSource();
        AnalyzeBPM();
    }

    void CreateNewCircle()
    {
        if (circleObject != null)
        {
            Destroy(circleObject);
        }

        circleObject = new GameObject("FilledCircle");
        circleObject.transform.SetParent(transform);
        circleObject.transform.localPosition = new Vector3(0, 0.51f, 0);
        circleObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        circleObject.transform.localScale = Vector3.one * circleScale;
        spriteRenderer = circleObject.AddComponent<SpriteRenderer>();
        
        if (circleSprite != null)
        {
            spriteRenderer.sprite = circleSprite;
        }
        else
        {
            Debug.LogWarning("Circle sprite is not assigned! Please assign a sprite in the inspector.");
        }
        
        spriteRenderer.color = circleColor;
        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        isCircleActive = true;
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
                
                if (circleObject != null)
                {
                    Destroy(circleObject);
                    isCircleActive = false;
                }
            }
        }
    }
    
    void AnalyzeBPM()
    {
        if (audioClip != null)
        {
            currentBPM = UniBpmAnalyzer.AnalyzeBpm(audioClip);
            
            if (currentBPM > 0)
            {
                CalculateBeatDuration();
                isBpmAnalyzed = true;
                CreateNewCircle();
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
        
        float pulseProgress = beatTimer / beatDuration;

        if (isCircleActive) 
        {
            float scaleProgress = 1f - pulseProgress;
            float currentScale = circleScale * scaleProgress;
            circleObject.transform.localScale = Vector3.one * currentScale;
            
            if (currentScale <= 0.01f || pulseProgress >= 1f)
            {
                Destroy(circleObject);
                isCircleActive = false;
            }
        }
        
        if (beatTimer >= beatDuration)
        {
            beatTimer = 0f;
            CreateNewCircle();
        }
    }
}