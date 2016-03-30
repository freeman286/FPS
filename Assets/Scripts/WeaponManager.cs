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

    [SerializeField]
    private GameObject weaponSwapSound;

    [SerializeField]
    private GameObject reloadSound;

    private int primaryMagsize;

    private int secondaryMagsize;

    private int swapping = 20;

    private int reloading = 100;

    public PlayerWeapon[] allWeapons;

    public Player player;

    void Awake () {

        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;

        primaryWeapon = allWeapons[Random.Range(0, allWeapons.Length - 1)];
        while (!primaryWeapon.primary) {
            primaryWeapon = allWeapons[Random.Range(0, allWeapons.Length - 1)];
        }
        secondaryWeapon = allWeapons[Random.Range(0, allWeapons.Length - 1)];
        while (secondaryWeapon.primary) {
            secondaryWeapon = allWeapons[Random.Range(0, allWeapons.Length - 1)];
        }
    }


    void Start()  {
        EquipWeapon(primaryWeapon);
        FillMags();
        
    }

            

    public void FillMags() {
        primaryMagsize = primaryWeapon.magSize;
        secondaryMagsize = secondaryWeapon.magSize;
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Q) && !IsReloading()) {
            SwitchWeapon();
        }

        if ((Input.GetKeyDown(KeyCode.R) || primaryMagsize == 0 || secondaryMagsize == 0) && reloading > 150)
        {
            Reload();
        }

        swapping += 1;
        SwitchingWeapons();
        Reloading();
        reloading += 1;
    }

    public PlayerWeapon GetCurrentWeapon() {
        return currentWeapon;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public GameObject GetCurrentFirePoint()
    {
        return currentFirePoint;
    }

    public GameObject GetcurrentShootSound()
    {
        return currentWeapon.shootSound;
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

        currentGraphics = _weapon.GetComponent<WeaponGraphics>();
        currentFirePoint = _firePoint;
        if (currentGraphics == null)
        {
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
            CmdSwitchingWeapons(true);
            PlayWeaponSwapSound();
        }

        if (!isLocalPlayer) {
            if (swapping == 10)
            {
                CmdSwitchingWeapons(true);
            }
            return;

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
    void CmdSwitchingWeapons(bool _toggle) {
        
        RpcRemoteSwitchingWeapons(_toggle);
    }

    [ClientRpc]
    void RpcRemoteSwitchingWeapons(bool _toggle) {

        if (currentWeapon == primaryWeapon)
        {
            EquipWeapon(secondaryWeapon);
        }
        else {
            EquipWeapon(primaryWeapon);
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

    public void Shooting()
    {
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

    }

    public bool CanShoot()
    {
        return ((currentWeapon == primaryWeapon && primaryMagsize != 0) || (currentWeapon == secondaryWeapon && secondaryMagsize != 0)) &&
               ((currentWeapon == primaryWeapon && reloading > primaryWeapon.reloadTime) || (currentWeapon == secondaryWeapon && reloading > secondaryWeapon.reloadTime) &&
               !IsReloading() &&
               !Swapping());
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
}