﻿using UnityEngine;

[System.Serializable]
public class PlayerWeapon : MonoBehaviour{
    public string name;
    public int damage;
    public int range;
    public int magSize;
    public int reloadTime;
    public int roundsPerShot;
    public int burst = 1;

    public float fireRate;
    public float spread;
    public float spreadWhileMoving;
    public float spreadWhileJumping;

    public float shootCooldown;

    public bool projectileWeapon;
    public float throwPower;

    public GameObject graphics;
    public GameObject shootSound;
    public GameObject firePoint;
    public GameObject projectile;
    public GameObject portHolder;
    public GameObject ejectionPort;
    public GameObject casing;

    public bool primary;
}
