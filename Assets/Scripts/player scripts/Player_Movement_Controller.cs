using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class Player_Movement_Controller : NetworkBehaviour
{
    private float walkingSpeed = 7.5f;
    private float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public GameObject playerLooking;
    public GameObject playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    private Player_Manager playerManager;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private PlayerInput playerInput;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    public float WalkingSpeed { get => walkingSpeed * playerManager.Archetype?.walkingSpeedModifier ?? walkingSpeed; set => walkingSpeed = value; }
    public float RunningSpeed { get => runningSpeed * playerManager.Archetype?.runningSpeedModifier ?? runningSpeed; set => runningSpeed = value; }

    private InputAction moveInput;
    private InputAction lookInput;
    private InputAction jumpInput;
    private InputAction sprintInput;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if(!IsOwner){
            return;
        }
        playerInput.enabled = true;

        moveInput = InputSystem.actions.FindAction("Move");
        lookInput = InputSystem.actions.FindAction("Look");
        jumpInput = InputSystem.actions.FindAction("Jump");
        sprintInput = InputSystem.actions.FindAction("Sprint");
    }

    private void Awake() {
        playerManager = GetComponent<Player_Manager>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerCamera.SetActive(true);
        }
    }

    public void AddForce(Vector3 force)
    {
        moveDirection += force;
    }


    // Update is called once per frame
    void Update()
    {
        if(!IsOwner){
            return;
        }
        if(!playerManager.IsReady){
            return;
        }
        if(!canMove){
            return;
        }
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = sprintInput.IsPressed();
        Vector2 input = moveInput.ReadValue<Vector2>();

        float movementDirectionY = moveDirection.y;
        moveDirection = ((forward * input.y) + (right * input.x)).normalized * (isRunning ? RunningSpeed : WalkingSpeed);

        if (jumpInput.IsPressed() && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed * playerManager.Archetype?.jumpSpeedModifier ?? jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Looking rotation

        rotationX += -lookInput.ReadValue<Vector2>().y * lookSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerLooking.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookInput.ReadValue<Vector2>().x * lookSpeed * Time.deltaTime, 0);

        // Animation
        if(animator != null){
            Vector3 horizontalMovement = new Vector3(moveDirection.x, 0, moveDirection.z);
            SyncAnimRpc(horizontalMovement.magnitude, characterController.isGrounded);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SyncAnimRpc(float hSpeed, bool isGrounded){
        if(animator != null){
            animator.SetFloat("H_Speed", hSpeed);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
}
