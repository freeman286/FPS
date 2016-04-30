using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed;
    

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    [SerializeField]
    private float lookSensitivity = 3f;

    float rotationY = 0F;

    private PlayerMotor motor;

    private Player player;

    public Animation walking;

    public WeaponManager weaponManager;

    private Rigidbody rb;

    void Start() {
        motor = GetComponent<PlayerMotor>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    int framesWithCursor = 0;
    void Update() {

        //Pressing Esc
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            framesWithCursor = 0;
        }
        else if (Input.GetButtonDown("Fire1") && framesWithCursor > 100) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            framesWithCursor = 0;
        }
        framesWithCursor += 1;

        //Speed Limit

        if (rb.velocity.y > 5) {
            rb.velocity = new Vector3(rb.velocity.x, 5, rb.velocity.z);
        }

        //WASD movement

        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;

        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;

        if ((!walking.isPlaying && _velocity != Vector3.zero && !weaponManager.IsMelee()) || (weaponManager.Swapping() && !weaponManager.IsMelee())) {
            walking.Play("Walking");
        }


        motor.Move(_velocity);

        //Mouse movement

        float _yRot = Input.GetAxisRaw("Mouse X");

        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;

        //Apply rotation
        motor.Rotate(_rotation);

        //Calculate camera rotation as a 3D vector (turning around)
        float _xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotationX = _xRot * lookSensitivity;

        //Apply camera rotation
        motor.RotateCamera(_cameraRotationX);

        //Jump

        if (Input.GetButton("Jump")) {
            motor.Jump();
        }
    }

    public void Reset() {
        walking.Play("Walking");
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Projectile") {
            collision.collider.GetComponent<ProjectileController>().Explode(collision);
        }
    }
}