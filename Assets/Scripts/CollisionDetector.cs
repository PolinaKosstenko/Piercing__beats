using UnityEngine.UI;
using UnityEngine;

public class SimpleSphereDestroyer : MonoBehaviour
{
    private float HP = 100f;
    public Image Bar;

    public AudioClip soundDestroy;
    public AudioClip soundNotDestroy;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hands"))
        {
            Debug.Log("Попадание!");
            AudioSource.PlayClipAtPoint(soundDestroy, transform.position);
            Destroy(gameObject);
        }
        else
        {
            AudioSource.PlayClipAtPoint(soundNotDestroy, transform.position);
            Debug.Log("Нет попадания");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Triggered");
            HP -= 5;
            Bar.fillAmount = HP / 100;
        }
        //Debug.Log("����� � �������: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Hands"))
        {
            Debug.Log("Попадание (триггер)!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Нет попадания (триггер)");
        }
    }
}