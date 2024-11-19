using UnityEngine;
using UnityEngine.InputSystem;

public class Spectator_Movement : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 7.5f;
    [SerializeField]
    private float lookSpeed = 2.0f;
    private bool isSpectating = false;

    public static Spectator_Movement Instance;

    private InputAction moveInput;
    private InputAction lookInput;
    private InputAction jumpInput;
    private InputAction sprintInput;

    private void Start() {
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this);
        }

        moveInput = InputSystem.actions.FindAction("Move");
        lookInput = InputSystem.actions.FindAction("Look");
        jumpInput = InputSystem.actions.FindAction("Jump");
        sprintInput = InputSystem.actions.FindAction("Sprint");
    }

    public void StartSpectating(Transform playerCameraPosition){
        isSpectating = true;
        transform.position = playerCameraPosition.position;
        transform.rotation = playerCameraPosition.rotation;
    }

    public void StopSpectating(){
        isSpectating = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isSpectating){
            return;
        }
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 up = transform.TransformDirection(Vector3.up);
        // Press Left Shift to run
        float curSpeedX = movementSpeed * moveInput.ReadValue<Vector2>().y;
        float curSpeedY = movementSpeed * moveInput.ReadValue<Vector2>().x;
        float curSpeedZ = movementSpeed * (jumpInput.IsPressed() ? 1 : 0);
        Vector3 moveDirection = (forward * curSpeedX) + (right * curSpeedY) + (up * curSpeedZ);
        moveDirection *= sprintInput.IsPressed() ? 2 : 1;

        // Move the controller
        transform.position += moveDirection * Time.deltaTime;

        // Player and Looking rotation
        //TODO: prevent player from looking up or down too far
        transform.RotateAround(transform.position, Vector3.up, lookInput.ReadValue<Vector2>().x * lookSpeed * Time.deltaTime);
        transform.RotateAround(transform.position, transform.right, -lookInput.ReadValue<Vector2>().y * lookSpeed * Time.deltaTime);
        

    }

}
