using UnityEngine;
using UnityEngine.Networking;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private string weaponLayerName = "Weapon";

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerShoot shoot;

    public PlayerWeapon primaryWeapon;

    public PlayerWeapon secondaryWeapon;

    private PlayerWeapon currentWeapon;

    private WeaponGraphics currentGraphics;

    private GameObject currentShootSound;

    private GameObject currentFirePoint;

    private GameObject currentEjectionPort;

    [SerializeField]
    private GameObject weaponSwapSound;

    [SerializeField]
    private GameObject reloadSound;

    private int primaryMagsize;

    private int secondaryMagsize;

    private int swapping = 20;

    private int reloading = 100;

    private int hitting = 100;

    public PlayerWeapon[] allWeapons;

    public Player player;

    public Camera cam;

    void Awake () {

        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;

        primaryWeapon = allWeapons[Random.Range(0, allWeapons.Length)];
        while (!primaryWeapon.primary) {
            primaryWeapon = allWeapons[Random.Range(0, allWeapons.Length)];
        }

        secondaryWeapon = allWeapons[Random.Range(0, allWeapons.Length)];
        while (secondaryWeapon.primary) {
            secondaryWeapon = allWeapons[Random.Range(0, allWeapons.Length)];
        }

    }


    void Start()  {
        FillMags();
        SwitchWeapon();
    }

            

    public void FillMags() {
        primaryMagsize = primaryWeapon.magSize;
        secondaryMagsize = secondaryWeapon.magSize;
    }


    void FixedUpdate() {

        if (System.DateTime.Now.Millisecond % 100 == 0) {
            foreach (var _playerID in GameManager.players.Keys) {
                CmdSwitchingWeapons(GameManager.players[_playerID].transform.name, GameManager.players[_playerID].GetComponent<WeaponManager>().currentWeapon.name, "");
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && !IsReloading() && shoot.currentBurst == 0 && isLocalPlayer) {
            SwitchWeapon();
        }

        if (((Input.GetKeyDown(KeyCode.R) || primaryMagsize == 0 || secondaryMagsize == 0) && reloading > 150) && !currentWeapon.meleeWeapon) {
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

    public GameObject GetcurrentShootSound()
    {
        return currentWeapon.shootSound;
    }

    public GameObject GetCurrentEjectionPort() {
        return currentEjectionPort;
    }

    public GameObject GetCurrentCasing() {
        return currentWeapon.casing;
    }

    public bool IsMelee() {
        return currentWeapon.meleeWeapon;
    }

    public void EquipPrimary() {
        EquipWeapon(primaryWeapon);
    }

    [Client]
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
        if (_weapon.portHolder != null) {
            GameObject _portHolder = (GameObject)Instantiate(_weapon.portHolder, weaponHolder.position, weaponHolder.rotation);
            _portHolder.transform.SetParent(weaponHolder);
            GameObject _ejectionPort = _portHolder.transform.GetChild(0).gameObject;
            currentEjectionPort = _ejectionPort;
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
            CmdSwitchingWeapons(transform.name, primaryWeapon.name, secondaryWeapon.name);
            PlayWeaponSwapSound();
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
    void CmdSwitchingWeapons(string _playerID, string _primaryWeapon, string _secondaryWeapon) {

        Player _player = GameManager.GetPlayer(_playerID);

        _player.weaponManager.RpcRemoteSwitchingWeapons(_player.weaponManager.currentWeapon.name, _primaryWeapon, _secondaryWeapon);
    }

    [ClientRpc]
    public void RpcRemoteSwitchingWeapons(string _currentWeapon, string _primaryWeapon, string _secondaryWeapon) {

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
                EquipWeapon(secondaryWeapon);
            }
            else {
                EquipWeapon(primaryWeapon);
            }
        } else {
            foreach (var weapon in allWeapons) {
                if (weapon.name == _currentWeapon) {
                    EquipWeapon(weapon);
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

        if (currentWeapon == primaryWeapon)
        {
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
        if (hitting < currentWeapon.shootCooldown / 4)
        {
            weaponHolder.transform.Rotate(12, 12, 0 * Time.deltaTime);
            weaponHolder.transform.Translate(-0.24f, 0, 0 * Time.deltaTime);
        }
        else if (hitting < currentWeapon.shootCooldown) {
            weaponHolder.transform.Rotate(-3, -3, 0 * Time.deltaTime);
            weaponHolder.transform.Translate(0.06f, 0, 0 * Time.deltaTime);
        } else if (!player.isDead) {
            weaponHolder.transform.rotation = cam.transform.rotation;
            weaponHolder.transform.position = cam.transform.position;
        }
    }
}