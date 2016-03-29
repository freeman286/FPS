using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour{
    public string name;
    public int damage;
    public int range;
    public int magSize;
    public int reloadTime;
    public int roundsPerShot;

    public float fireRate;
    public float spread;
    public float spreadWhileMoving;
    public float spreadWhileJumping;

    public int shootCooldown;

    public GameObject graphics;
    public GameObject shootSound;
    public GameObject firePoint;

    public bool primary;
}
