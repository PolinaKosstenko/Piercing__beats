using UnityEngine;

public class SphereController : MonoBehaviour
{
    private float initialScale;
    private float lifeTime;
    private float maxLifeTime;
    private int difficulty;
    
    public void Initialize(float scale, float beatDuration, int diff)
    {
        initialScale = scale;
        maxLifeTime = beatDuration;
        difficulty = diff;
        lifeTime = 0f;
    }
    
    void Update()
    {
        lifeTime += Time.deltaTime;
        
        // Прогресс жизни (0..1)
        float lifeProgress = lifeTime / maxLifeTime;
        
        if (lifeProgress >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        
        // Вычисляем масштаб
        float pulseProgress = (lifeTime * 2 * difficulty) / maxLifeTime;
        float scaleProgress = Mathf.Clamp01(1f - pulseProgress);
        float currentScale = initialScale * scaleProgress;
        
        // Применяем масштаб
        transform.localScale = Vector3.one * currentScale;
        
        // Уничтожаем если слишком маленькая
        if (currentScale <= 0.01f)
        {
            Destroy(gameObject);
        }
    }
}