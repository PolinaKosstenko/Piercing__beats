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
        if (other.gameObject.CompareTag("SphereTag"))
        {
            if (other.gameObject.GetComponent<Transform>().localScale.x <= 0.1)
            {
                Debug.Log("Нет попадания");
                AudioSource.PlayClipAtPoint(soundNotDestroy, transform.position);

            }
            else {
                Debug.Log("Попадание!");
                AudioSource.PlayClipAtPoint(soundDestroy, transform.position);
            }
            // Destroy(gameObject);
        }
    }
}