using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    // Коллизия при столкновении (для 3D)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Anchor"))
        {
            Debug.Log("Коллизия с кружком");
        }
        //Debug.Log("Столкнулся с: " + collision.gameObject.tag);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Anchor"))
        {
            Debug.Log("Триггер с кружком");

        }
        //Debug.Log("Вошел в триггер: " + other.gameObject.name);
    }
}