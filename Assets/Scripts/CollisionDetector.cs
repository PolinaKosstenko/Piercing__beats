using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    // Коллизия при столкновении (для 3D)
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Столкнулся с: " + collision.gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Вошел в триггер: " + other.gameObject.name);
    }
}