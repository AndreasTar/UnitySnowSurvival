using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 5f;
    Vector3 velocity;

    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public float turnSmoothTime = 0.5f;
    float turnSmoothVel;

    public CharacterController cC;
    public Transform groundCheck;

    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    public bool debugMode;

    Vector3 direction = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        // input lines, horizontal is x, vertical is z
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // isGrounded check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -0.1f;
        }

        if(isGrounded || debugMode)
        {
            direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

        }

        // gravity logic
        velocity.y += gravity * Time.deltaTime;
        cC.Move(velocity * Time.deltaTime);

        if (direction.magnitude >= 0.1f && (isGrounded || debugMode))
        {
            // rotation smoothing
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVel, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
        }

        // moving
        cC.Move(direction * moveSpeed * Time.deltaTime);
    }
}
