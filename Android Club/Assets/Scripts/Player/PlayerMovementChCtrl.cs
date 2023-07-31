using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementChCtrl : MonoBehaviour
{
    //Serialized variables
    [SerializeField] float groundSpeed = 5;
    [SerializeField] float groundSpeedMax = 7;
    [SerializeField] float acceleration = 1.5f;
    [SerializeField] float airSpeed = 3;
    [SerializeField] float jumpHeight = 6;
    [SerializeField] float coyoteTime = .125f;
    [SerializeField] float wallRunUpFoce = .75f;

    [Header("Mouse")]
    [SerializeField] float sensitivity = 50;

    [Header("References")]
    [SerializeField] Transform playerCam;

    //public Variables
    public bool grounded;
    public bool isEnabled;

    //Private References 
    CharacterController controller;

    //Private
    float x, z;
    bool jumping;
    Vector3 moveDirection;
    float coyoteTimeRn;
    float xRotation;
    float speedRn;
    float upForceNow;
    Vector2 groundJumpDir = Vector2.zero;
    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        upForceNow = wallRunUpFoce;
    }

    void Update()
    {
        MyInput();
        Look();
    }

    void FixedUpdate()
    {
        if (isEnabled)
            Movement();
    }

    void MyInput()
    {
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        jumping = Input.GetKey(KeyCode.Space);
    }

    void Movement()
    {
        speedRn = Mathf.Abs(x) == 1 || Mathf.Abs(z) == 1 ? Mathf.Lerp(speedRn, groundSpeedMax, Time.deltaTime * acceleration) : groundSpeed;
        if (grounded)
        {
            moveDirection = new Vector3(x * groundSpeed, -.75f, z * speedRn);
            moveDirection = transform.TransformDirection(moveDirection);

            groundJumpDir = new Vector2(x, z);
        }
        else
        {
            moveDirection.x = x * (Mathf.Sign(groundJumpDir.x) == Mathf.Sign(x) && groundJumpDir.x != 0 ? speedRn : airSpeed);
            moveDirection.z = z * (Mathf.Sign(groundJumpDir.y) == Mathf.Sign(z) && groundJumpDir.y != 0 ? speedRn : airSpeed);
            moveDirection = transform.TransformDirection(moveDirection);

            coyoteTimeRn += Time.deltaTime;
        }

        if (wallRunExitVector != Vector3.zero)
        {
            moveDirection.x += wallRunExitVector.x;
            moveDirection.z += wallRunExitVector.z;
        }

        if (jumping && coyoteTimeRn <= coyoteTime)
        {
            moveDirection.y = jumpHeight;
        }

        moveDirection.y -= 20 * Time.deltaTime;
        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

        if (grounded)
        {
            wallRunExitVector = Vector3.zero;
            upForceNow = wallRunUpFoce;
            coyoteTimeRn = 0;
        }
    }

    public void ResetMovement()
    {
        moveDirection = Vector3.zero;
        wallRunExitVector = Vector3.zero;
    }

    public void WallRun(Vector3 wallRunDir, float wallRunSpeed)
    {
        controller.Move(wallRunDir * Time.deltaTime * wallRunSpeed + transform.up * upForceNow * Time.deltaTime);
        upForceNow -= Time.deltaTime;
    }

    Vector3 wallRunExitVector;
    public void ResetWallRun(Vector3 exitDirection, float speed)
    {
        wallRunExitVector = exitDirection * speed;
        groundJumpDir = new Vector2(x, z);
    }

    public void AddWallRunSpeedModifier()
    {
        wallRunExitVector += transform.forward * jumpHeight / 2;
        coyoteTimeRn = 0;
    }

    private float desiredX;
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, playerCam.eulerAngles.z);
        transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }
}
