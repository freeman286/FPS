using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour{
    public string name;
    public int damage;
    public int range;
    public int magSize;
    public int reloadTime;
    public int roundsPerShot;
    public int burst = 1;
    public int force;
    public float speed = 1;

    public float fireRate;
    public float spread;
    public float spreadWhileMoving;
    public float spreadWhileJumping;

    public float shootCooldown;

    public bool projectileWeapon;
    public float throwPower;

    public bool meleeWeapon;

    public bool reflectable;
    public BoxCollider col;

    public bool dualWieldable;

    public GameObject graphics;
    public GameObject shootSound;
    public GameObject firePoint;
    public GameObject projectile;
    public GameObject portHolder;
    public GameObject ejectionPort;
    public GameObject casing;

    public int barrels;

    public bool primary;

    public float cameraRotationLimit = 50f;
}
