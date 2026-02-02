using UnityEngine;

public class SphereController : MonoBehaviour
{
    private float initialScale;
    private float targetScale = 0f;
    private float scaleDownSpeed;
    private Color sphereColor;
    private float difficulty;
    

    // Фиксированное время сжатия для всех кругов
    private const float FIXED_CONTRACT_TIME = 1.5f;
    private float lifeTimer;
    private float maxLifeTime;

    public void Initialize(float startScale, float noteDuration, int diff, Color color)
    {
        initialScale = startScale;
        difficulty = diff;
        sphereColor = color;

        // Рассчитываем фиксированную скорость сжатия
        scaleDownSpeed = initialScale / FIXED_CONTRACT_TIME;

        // Устанавливаем максимальное время жизни
        maxLifeTime = FIXED_CONTRACT_TIME + 0.5f;
        lifeTimer = 0f;

        // Настраиваем визуал
        SetupVisuals();

        // Логирование для отладки
        Debug.Log($"Sphere created: scale={initialScale}, contractTime={FIXED_CONTRACT_TIME}s, speed={scaleDownSpeed:F3}/s");
    }

    void SetupVisuals()
    {
        // Настройка материала
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = sphereColor;
            renderer.material.SetFloat("_Metallic", 0.5f);
            renderer.material.SetFloat("_Glossiness", 0.5f);
        }

        // Устанавливаем начальный масштаб
        transform.localScale = Vector3.one * initialScale;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;

        // Медленно уменьшаем масштаб с фиксированной скоростью
        if (transform.localScale.x > targetScale)
        {
            float deltaScale = scaleDownSpeed * Time.deltaTime;
            float newScale = Mathf.Max(transform.localScale.x - deltaScale, targetScale);
            transform.localScale = Vector3.one * newScale;

            // Уничтожаем, если сжался полностью
            if (newScale <= targetScale + 0.01f)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Автоматическое уничтожение по таймеру (на всякий случай)
        if (lifeTimer >= maxLifeTime)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Очистка материала
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Destroy(renderer.material);
        }
    }
}