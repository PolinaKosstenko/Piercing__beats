using UnityEngine;
using UnityEngine.InputSystem;

public class jump : MonoBehaviour
{
    bool isGrounded = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        //if (isGrounded)
        //{
            Debug.Log("hahaha");
            //rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        //}
    }
}
