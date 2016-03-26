using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "smg";
    public int damage = 10;
    public int range = 200;

    public float fireRate = 10f;

    public GameObject graphics;
    public GameObject shootSound;
}
