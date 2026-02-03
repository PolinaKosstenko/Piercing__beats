using UnityEngine.UI;
using UnityEngine;

public class SimpleSphereDestroyer : MonoBehaviour
{
    private float HP = 100f;
    public Image Bar;

    public AudioClip soundDestroy;
    public AudioClip soundNotDestroy;

    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Triggered");
            HP -= 5;
            Bar.fillAmount = HP / 100;
        }
        //Debug.Log("����� � �������: " + other.gameObject.name);
        if (gameObject.CompareTag("SphereTag") && other.gameObject.CompareTag("SphereTag"))
        {
            return;
        }
        
         if (other.gameObject.CompareTag("SphereTag"))
        {
            if (other.gameObject.GetComponent<Transform>().localScale.x <= 0.1)
            {
                Debug.Log("Нет попадания");
                AudioSource.PlayClipAtPoint(soundNotDestroy, transform.position);
                // useless but its okay i guess
            }
            else {
                Debug.Log("Попадание!");
                AudioSource.PlayClipAtPoint(soundDestroy, transform.position);
                Destroy(other.gameObject);
                ApplyDamageToRobot();
            }
        }
    }
    
    void ApplyDamageToRobot()
    {
        Transform robotTransform = transform;
        while (robotTransform != null)
        {
            HpManager hpManager = robotTransform.GetComponent<HpManager>();
            if (hpManager != null)
            {
                hpManager.TakeDamage(1f);
                return;
            }
            hpManager = robotTransform.GetComponentInChildren<HpManager>();
            if (hpManager != null)
            {
                hpManager.TakeDamage(1f);
                return;
            }
            robotTransform = robotTransform.parent;
        }

        HpManager[] allHpManagers = FindObjectsOfType<HpManager>();
        if (allHpManagers.Length > 0)
            allHpManagers[0].TakeDamage(1f);
        else
            Debug.LogWarning("No HpManager component found in scene!");
    }
}