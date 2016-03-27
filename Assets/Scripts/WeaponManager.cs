using UnityEngine;
using UnityEngine.Networking;

public class WeaponManager : NetworkBehaviour {

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerShoot shoot;

    [SerializeField]
    private PlayerWeapon primaryWeapon;

    [SerializeField]
    private PlayerWeapon secondaryWeapon;

    private PlayerWeapon currentWeapon;

    private WeaponGraphics currentGraphics;

    private GameObject currentShootSound;

    private GameObject currentFirePoint;

    private int reloading = 100;

    void Start () {
        EquipWeapon(primaryWeapon);
        Debug.Log(primaryWeapon.graphics);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            SwitchWeapon();
        }
        if (reloading < 10) {
            weaponHolder.transform.Rotate(3, 0, 0 * Time.deltaTime);
        } else if (reloading < 20) {
            weaponHolder.transform.Rotate(-3, 0, 0 * Time.deltaTime);
        } else {
            weaponHolder.transform.Rotate(0, 0, 0);
        }


        if (currentWeapon == primaryWeapon && reloading == 10) {
            EquipWeapon(secondaryWeapon);
        } else if (reloading == 10) {
            EquipWeapon(primaryWeapon);
        }

        reloading += 1;
    }

    public PlayerWeapon GetCurrentWeapon() {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics() {
        return currentGraphics;
    }

    public GameObject GetCurrentFirePoint() {
        return currentFirePoint;
    }

    public GameObject GetcurrentShootSound() {
        return currentWeapon.shootSound;
    }

    void EquipWeapon(PlayerWeapon _weapon) {

        foreach (Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }

        currentWeapon = _weapon;

        GameObject _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponIns.transform.SetParent(weaponHolder);
        GameObject _firePoint = (GameObject)Instantiate(_weapon.firePoint, weaponHolder.position, weaponHolder.rotation);
        _firePoint.transform.SetParent(weaponHolder);
        
        currentGraphics = _weapon.GetComponent<WeaponGraphics>();
        currentFirePoint = _firePoint;
        if (currentGraphics == null) {
            Debug.LogError("No WeaponGraphics for weapon " + _weaponIns.name);
        }

        if (isLocalPlayer) {
            Util.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer(weaponLayerName));
            Util.SetLayerRecursively(_firePoint, LayerMask.NameToLayer(weaponLayerName));
        }
    }

    void SwitchWeapon() {
        shoot.CancelInvoke("Shoot");
        if (reloading > 50) {
            reloading = 0;
        }
    }

}
