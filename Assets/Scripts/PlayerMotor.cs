
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour {

    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;

    [SerializeField]
    private float cameraRotationLimit = 50f;

    private Rigidbody rb;

    public bool isGrounded = true;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    public void Move (Vector3 _velocity) {
        velocity = _velocity;
    }

    public void Rotate(Vector3 _rotation) {
        rotation = _rotation;
    }

    public void RotateCamera(float _cameraRotationX) {
        cameraRotationX = _cameraRotationX;
    }

    void FixedUpdate () {
        PerformMovement();
        PerformRotation();
    }

    void PerformMovement() {
        if (velocity != Vector3.zero) {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            currentCameraRotationX -= cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

            cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }

    public void Jump() {
        if (isGrounded) {
            rb.AddForce(Vector3.up * 350f);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collisionInfo) {
        if (collisionInfo.collider.tag == "Surface") {
            isGrounded = true;     
        } 
    }
}
