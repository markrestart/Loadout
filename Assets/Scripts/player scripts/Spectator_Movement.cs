using UnityEngine;

public class Spectator_Movement : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 7.5f;
    [SerializeField]
    private float lookSpeed = 2.0f;
    private bool isSpectating = false;

    public static Spectator_Movement Instance;

    private void Start() {
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this);
        }
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
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = movementSpeed * Input.GetAxis("Vertical");
        float curSpeedY = movementSpeed * Input.GetAxis("Horizontal");
        Vector3 moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Move the controller
        transform.position += moveDirection * Time.deltaTime;

        // Player and Looking rotation
        transform.RotateAround(transform.position, Vector3.up, Input.GetAxis("Mouse X") * lookSpeed);
        transform.RotateAround(transform.position, transform.right, -Input.GetAxis("Mouse Y") * lookSpeed);
        

    }

}
