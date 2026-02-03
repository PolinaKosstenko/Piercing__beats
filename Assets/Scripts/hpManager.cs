using UnityEngine;
using System.Collections;

public class HpManager : MonoBehaviour
{
    [SerializeField] private int sphereCount;
    [SerializeField] private float destroyDelay = 7f;

    private float _hp;
    private bool _isAlive = true;
    private bool _initialized;
    private bool _destroyScheduled;

    public float HP => _hp;
    public float MaxHp => _initialized ? sphereCount * 0.4f : 0f;
    public bool IsAlive => _isAlive;

    void Start()
    {
        TryInitFromSphereCreator();
    }

    void Update()
    {
        if (!_initialized)
            TryInitFromSphereCreator();
    }

    void TryInitFromSphereCreator()
    {
        if (_initialized) return;

        CreateSphere creator = FindFirstObjectByType<CreateSphere>();
        if (creator != null && creator.NoteSequence != null && creator.NoteSequence.Length > 0)
        {
            sphereCount = creator.NoteSequence.Length;
            _hp = sphereCount * 0.4f;
            _initialized = true;
            Debug.LogWarning(_hp);
        }
        else if (sphereCount > 0)
        {
            _hp = sphereCount * 0.4f;
            _initialized = true;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!_initialized)
            TryInitFromSphereCreator();

        _hp -= amount;
        if (_hp <= 0f)
            OnDeath();
    }

    void OnDeath()
    {
        _isAlive = false;
        if (!_destroyScheduled)
        {
            _destroyScheduled = true;
            StartCoroutine(DestroyAfterDelay());
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
