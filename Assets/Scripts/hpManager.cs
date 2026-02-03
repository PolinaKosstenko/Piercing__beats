using UnityEngine;
using System.Collections;

public class HpManager : MonoBehaviour
{
    [SerializeField] private int sphereCount;
    [SerializeField] private float destroyDelay = 0f;

    public float _hp;
    private bool _isAlive = true;
    private bool _initialized;
    private bool _destroyScheduled;

    public float HP => _hp;
    public float MaxHp => 10;
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
        _hp = 10;
        _initialized = true;
    }

    public void TakeDamage(float amount)
    {
        _hp -= 2;
        if (_hp <= 0f)
            OnDeath();
    }

    void OnDeath()
    {
        GameObject.FindWithTag("RobotSpawner").GetComponent<RobotSpawner>().SpawnNext();  
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
