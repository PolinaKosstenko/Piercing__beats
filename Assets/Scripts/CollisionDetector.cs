using UnityEngine.UI;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    private float HP = 100f;
    public Image Bar;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Anchor"))
        {
            Debug.Log("Colision");
        }
        //Debug.Log("���������� �: " + collision.gameObject.tag);
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
    }
}