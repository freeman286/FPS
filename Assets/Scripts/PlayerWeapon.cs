using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour{
    public string name;
    public int damage;
    public int range;

    public float fireRate;
    public float spread;
    public float spreadWhileMoving;
    public float spreadWhileJumping;

    public GameObject graphics;
    public GameObject shootSound;
    public GameObject firePoint;
}
