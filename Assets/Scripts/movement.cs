using UnityEngine;

public class movement : MonoBehaviour
{
    public float speed = 5f;

    void Start()
    {
        Debug.Log("Игрок создан!");
    }

    void Update()
    {
        // Движение с помощью клавиш
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        transform.Translate(moveX, 0, moveZ);
    }
}