
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

    public float jumpForce = 280f;

    public WeaponManager weaponManager;

    private Rigidbody rb;

    public bool isGrounded = true;

    private Vector3 lastPos;

    private Vector3 currentPos;

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

    void Update () {
        cameraRotationLimit = weaponManager.GetCurrentWeapon().cameraRotationLimit;
    }

    void FixedUpdate () {
        PerformMovement();
        PerformRotation();
        lastPos = rb.transform.position;
    }

    void PerformMovement() {
        if (velocity != Vector3.zero) {
            rb.MovePosition(rb.position + (rb.velocity + velocity) * weaponManager.GetCurrentWeapon().speed * Time.fixedDeltaTime / (Mathf.Pow(1.25f, (rb.mass * 2) - 1)));
        }
    }

    void PerformRotation() {
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
            rb.AddForce(Vector3.up * jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionStay(Collision collision) {
        if (collision.collider.tag == "Surface") {
            isGrounded = true;     
        }
    }

    void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Surface") {
            isGrounded = false;
        }
    }

    public bool IsGrounded () {
        return isGrounded;
    }

    public bool IsMoving() {
        currentPos = rb.position;
        return currentPos != lastPos;
    }
}
