using UnityEngine;

public class SphereController : MonoBehaviour
{
    private float initialScale;
    private float lifeTime;
    private float maxLifeTime;
    private int difficulty;
    private Renderer sphereRenderer;
    private Color initialColor;
    
    public void Initialize(float scale, float beatDuration, int diff, Color color)
    {
        initialScale = scale;
        maxLifeTime = beatDuration;
        difficulty = diff;
        lifeTime = 0f;
        
        sphereRenderer = GetComponent<Renderer>();
        if (sphereRenderer != null)
        {
            initialColor = color;
            sphereRenderer.material.color = color;
        }
    }
    
    void Update()
    {
        lifeTime += Time.deltaTime;
        
        float lifeProgress = lifeTime / maxLifeTime;
        
        if (lifeProgress >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        
        float pulseProgress = (lifeTime * 2 * difficulty) / maxLifeTime;
        float scaleProgress = Mathf.Clamp01(1f - pulseProgress);
        float currentScale = initialScale * scaleProgress;
        
        transform.localScale = Vector3.one * currentScale;
        
        if (sphereRenderer != null)
        {
            Color currentColor = initialColor;
            currentColor.a = scaleProgress;
            sphereRenderer.material.color = currentColor;
        }
        
        if (currentScale <= 0.01f)
        {
            Destroy(gameObject);
        }
    }
}