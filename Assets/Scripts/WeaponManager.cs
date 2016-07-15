using UnityEngine;
using UnityEngine.Networking;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private Transform altWeaponHolder;

    [SerializeField]
    private PlayerShoot shoot;

    public PlayerWeapon primaryWeapon;

    public PlayerWeapon secondaryWeapon;

    private PlayerWeapon currentWeapon;

    private WeaponGraphics currentGraphics;

    private GameObject currentShootSound;

    private GameObject currentFirePoint;

    private GameObject altCurrentFirePoint;

    private GameObject currentEjectionPort;

    private GameObject altCurrentEjectionPort;

    [SerializeField]
    private GameObject weaponSwapSound;

    [SerializeField]
    private GameObject reloadSound;

    private int primaryMagsize;

    private int secondaryMagsize;

    public int swapping = 20;

    private int reloading = 100;

    private int hitting = 100;

    private bool dualWielding = false;

    public PlayerWeapon[] allWeapons;

    public Player player;

    public Camera cam;

    void Awake () {

        if (Camera.main.GetComponent<PlayerInfo>() != null) {
            primaryWeapon = Camera.main.GetComponent<PlayerInfo>().GetPrimaryWeapon();
            secondaryWeapon = Camera.main.GetComponent<PlayerInfo>().GetSecondaryWeapon();
            dualWielding = Camera.main.GetComponent<PlayerInfo>().DualWielding();
        }
    }


    void Start()  {
        FillMags();
        SwitchWeapon();
    }

            

    public void FillMags() {
        primaryMagsize = primaryWeapon.magSize;
        secondaryMagsize = secondaryWeapon.magSize;
        if (dualWielding) {
            secondaryMagsize *= 2;
        }
    }


    void FixedUpdate() {

        if (player.timeSinceSpawned == -1) {
            foreach (var _playerID in GameManager.players.Keys) {
                CmdSwitchingWeapons(GameManager.players[_playerID].transform.name, GameManager.players[_playerID].GetComponent<WeaponManager>().currentWeapon.name, "", false);              
            }
            currentWeapon = secondaryWeapon;
            swapping = 0;
        } else if (player.timeSinceSpawned == 0) {
            currentWeapon = secondaryWeapon;
            swapping = 0;
        }



        if (Input.GetKeyDown(KeyCode.Q) && !IsReloading() && shoot.currentBurst == 0 && isLocalPlayer) {
            SwitchWeapon();
        }

        if (((Input.GetKeyDown(KeyCode.R) || primaryMagsize == 0 || secondaryMagsize == 0) && reloading > 150) && !currentWeapon.meleeWeapon && shoot.currentBurst == 0) {
            Reload();
        }

        swapping += 1;
        SwitchingWeapons();
        Reloading();
        reloading += 1;
        if (currentWeapon.meleeWeapon) {
            Hitting();
            hitting += 1;
        }
    }

    public PlayerWeapon GetCurrentWeapon() {
        return currentWeapon;
    }

    public PlayerWeapon GetOtherWeapon() {
        if (currentWeapon == primaryWeapon)
        {
            return secondaryWeapon;
        }
        else {
            return primaryWeapon;
        }
    }

    public WeaponGraphics GetCurrentGraphics() {
        return currentGraphics;
    }

    public GameObject GetCurrentProjectile() {
        return currentWeapon.projectile;
    }

    public GameObject GetCurrentFirePoint()
    {
        return currentFirePoint;
    }

    public GameObject GetAltCurrentFirePoint()
    {
        return altCurrentFirePoint;
    }

    public GameObject GetcurrentShootSound()
    {
        return currentWeapon.shootSound;
    }

    public GameObject GetCurrentEjectionPort() {
        return currentEjectionPort;
    }

    public GameObject GetAltCurrentEjectionPort()
    {
        return altCurrentEjectionPort;
    }

    public GameObject GetCurrentCasing() {
        return currentWeapon.casing;
    }

    public bool IsMelee() {
        return currentWeapon.meleeWeapon;
    }

    public bool IsDualWielding() {
        return !currentWeapon.primary && dualWielding;
    }

    public void EquipPrimary() {
        EquipWeapon(primaryWeapon, false);
    }

    public void StopDualWielding() {
        CmdSwitchingWeapons(transform.name, primaryWeapon.name, "", false);
    }

    [Client]
    void EquipWeapon(PlayerWeapon _weapon, bool _dualWielding) {

        shoot.daulGun = false;

        foreach (Transform child in weaponHolder) {
            if (child != altWeaponHolder) {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in altWeaponHolder)
        {
            Destroy(child.gameObject);
        }

        currentWeapon = _weapon;

        GameObject _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponIns.transform.SetParent(weaponHolder);
        GameObject _firePoint = (GameObject)Instantiate(_weapon.firePoint, weaponHolder.position, weaponHolder.rotation);
        _firePoint.transform.SetParent(weaponHolder);
        if (_weapon.portHolder != null) {
            GameObject _portHolder = (GameObject)Instantiate(_weapon.portHolder, weaponHolder.position, weaponHolder.rotation);
            _portHolder.transform.SetParent(weaponHolder);
            GameObject _ejectionPort = _portHolder.transform.GetChild(0).gameObject;
            currentEjectionPort = _ejectionPort;
        }

        if (_dualWielding && !currentWeapon.primary) {
            GameObject _altWeaponIns = (GameObject)Instantiate(_weapon.graphics, altWeaponHolder.position, altWeaponHolder.rotation);
            _altWeaponIns.transform.SetParent(altWeaponHolder);
            GameObject _altFirePoint = (GameObject)Instantiate(_weapon.firePoint, altWeaponHolder.position, altWeaponHolder.rotation);
            _altFirePoint.transform.SetParent(altWeaponHolder);
            if (_weapon.portHolder != null) {
                GameObject _altPortHolder = (GameObject)Instantiate(_weapon.portHolder, altWeaponHolder.position, altWeaponHolder.rotation);
                _altPortHolder.transform.SetParent(altWeaponHolder);
                GameObject _altEjectionPort = _altPortHolder.transform.GetChild(0).gameObject;
                altCurrentEjectionPort = _altEjectionPort;
            }

            if (isLocalPlayer) {
                Util.SetLayerRecursively(_altWeaponIns, LayerMask.NameToLayer(weaponLayerName));
                Util.SetLayerRecursively(_altFirePoint, LayerMask.NameToLayer(weaponLayerName));
            }

            altCurrentFirePoint = _altFirePoint;
        }

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

    [Client]
    void SwitchWeapon() {
        shoot.CancelInvoke("Shoot");
        if (swapping > 50)
        {
            swapping = 0;
        }
    }

    void SwitchingWeapons() {

        if (swapping == 10) {
            CmdSwitchingWeapons(transform.name, primaryWeapon.name, secondaryWeapon.name, dualWielding);
            PlayWeaponSwapSound();
            hitting = 100;
        }

        if (swapping < 10)
        {
            weaponHolder.transform.Rotate(3, 0, 0 * Time.deltaTime);
        }
        else if (swapping < 20)
        {
            weaponHolder.transform.Rotate(-3, 0, 0 * Time.deltaTime);
        }

    }

    [Command]
    void CmdSwitchingWeapons(string _playerID, string _primaryWeapon, string _secondaryWeapon, bool _dualWielding) {

        Player _player = GameManager.GetPlayer(_playerID);

        _player.weaponManager.RpcRemoteSwitchingWeapons(_player.weaponManager.currentWeapon.name, _primaryWeapon, _secondaryWeapon, _dualWielding);
    }

    [ClientRpc]
    public void RpcRemoteSwitchingWeapons(string _currentWeapon, string _primaryWeapon, string _secondaryWeapon, bool _dualWielding) {

        if (_secondaryWeapon != "")
        {
            foreach (var weapon in allWeapons)
            {
                if (weapon.name == _primaryWeapon)
                {
                    primaryWeapon = weapon;
                }
                if (weapon.name == _secondaryWeapon)
                {
                    secondaryWeapon = weapon;
                }
            }
            if (_currentWeapon == _primaryWeapon)
            {
                EquipWeapon(secondaryWeapon, _dualWielding);
            }
            else {
                EquipWeapon(primaryWeapon, _dualWielding);
            }
        } else {
            foreach (var weapon in allWeapons) {
                if (weapon.name == _currentWeapon) {
                    EquipWeapon(weapon, _dualWielding);
                }
            }

        }

    }

    void PlayWeaponSwapSound() {
        AudioSource _weaponSwapSound = (AudioSource)Instantiate(
                weaponSwapSound.GetComponent<AudioSource>(),
                gameObject.transform.position,
                new Quaternion(0, 0, 0, 0)
            );
        _weaponSwapSound.Play();
        Destroy(_weaponSwapSound.gameObject, 1f);
    }

    public bool Swapping() {
        return swapping < 20;
    }

    [Client]
    void Reload()
    {


        shoot.CancelInvoke("Shoot");
        if (reloading > 50)
        {
            reloading = 0;
        }


    }

    [Client]
    void Reloading()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (currentWeapon == primaryWeapon && primaryMagsize != primaryWeapon.magSize && reloading == primaryWeapon.reloadTime) {
            primaryMagsize = primaryWeapon.magSize;
            PlayReloadSound();
        }
        if (currentWeapon == secondaryWeapon && secondaryMagsize != secondaryWeapon.magSize && reloading == secondaryWeapon.reloadTime) {
            secondaryMagsize = secondaryWeapon.magSize;
            if (dualWielding) {
                secondaryMagsize *= 2;
            }
            PlayReloadSound();
        }


        if (reloading < currentWeapon.reloadTime / 2)
        {
            weaponHolder.transform.Rotate(1, 0, 0 * Time.deltaTime);
        }
        else if (reloading < currentWeapon.reloadTime)
        {
            weaponHolder.transform.Rotate(-1, 0, 0 * Time.deltaTime);
        }
    }

    public void Shooting() {
        if (!isLocalPlayer)
        {
            return;
        }

        if (currentWeapon == primaryWeapon) {
            primaryMagsize -= 1;
        }
        else {
            secondaryMagsize -= 1;
        }

        if (currentWeapon.meleeWeapon) {
            hitting = 0;
        }
    }

    public bool CanShoot()
    {
        return (((currentWeapon == primaryWeapon && primaryMagsize != 0) || (currentWeapon == secondaryWeapon && secondaryMagsize != 0)) &&
               ((currentWeapon == primaryWeapon && reloading > primaryWeapon.reloadTime) || (currentWeapon == secondaryWeapon && reloading > secondaryWeapon.reloadTime) &&
               !IsReloading() &&
               !Swapping())
               || currentWeapon.meleeWeapon);
    }

    public bool IsReloading() {
        return reloading <= currentWeapon.reloadTime;
    }

    void PlayReloadSound() {
        AudioSource _reloadSound = (AudioSource)Instantiate(
                reloadSound.GetComponent<AudioSource>(),
                gameObject.transform.position,
                new Quaternion(0, 0, 0, 0)
            );
        _reloadSound.Play();
        Destroy(_reloadSound.gameObject, 1f);
    }

    public void Hitting() {
        if (hitting < currentWeapon.shootCooldown / 4) {
            weaponHolder.transform.Rotate(12, 12, 0 * Time.deltaTime);
            weaponHolder.transform.Translate(-0.24f, 0, 0 * Time.deltaTime);
        } else if (hitting < currentWeapon.shootCooldown) {
            weaponHolder.transform.Rotate(-3, -3, 0 * Time.deltaTime);
            weaponHolder.transform.Translate(0.06f, 0, 0 * Time.deltaTime);
        } else if (!player.isDead) {
            weaponHolder.transform.rotation = cam.transform.rotation;
            weaponHolder.transform.position = cam.transform.position;
        }

        if (currentWeapon.reflectable) {
            BoxCollider c = currentWeapon.col as BoxCollider;
            if (hitting < currentWeapon.shootCooldown) {
                c.size = new Vector3(10, c.size.y, c.size.z);
            } else {
                c.size = new Vector3(3, c.size.y, c.size.z);
            }
        }
    }
}