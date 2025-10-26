using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    // Коллизия при столкновении (для 3D)
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Столкнулся с: " + collision.gameObject.name);
    }
}