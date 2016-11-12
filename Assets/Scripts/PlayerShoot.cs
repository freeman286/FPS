using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour {

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    private PlayerWeapon currentWeapon;

    private WeaponManager weaponManager;

    private Rigidbody rb;

    public Rigidbody PlayerRB;

    private GameObject shootSound;

    private float shootCooldown = 200;

    [SerializeField]
    private PlayerMotor motor;

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private Transform altWeaponHolder;

    private int shooting = 100;

    public bool daulGun = false;

    public int currentBurst = 0;

    private int barrel = 0;


    void Start() {
        if (cam == null)
        {
            Debug.Log("No camera referenced");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    void FixedUpdate() {
                
        currentWeapon = weaponManager.GetCurrentWeapon();
        shootCooldown += 1;

        if (weaponManager.IsDualWielding() && currentWeapon.shootCooldown < 20) {
            shootCooldown += 0.2f;
        }

        if (currentWeapon.fireRate <= 0 && shootCooldown > currentWeapon.shootCooldown) {
            if (Input.GetButtonDown("Fire1")) {
                Shoot();
                shootCooldown = currentWeapon.shootOffset;
                currentBurst = 0;
            }
        } else if (currentWeapon.burst > 1) {
            if (Input.GetButtonDown("Fire1") && currentBurst == 0 && !weaponManager.IsReloading() && shootCooldown > currentWeapon.shootCooldown) {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }

            if (currentBurst >= currentWeapon.burst) {
                CancelInvoke("Shoot");
                shootCooldown = currentWeapon.shootOffset;
                currentBurst = 0;
            }
        } else if (currentWeapon.fireRate > 0) {
            if (Input.GetButtonDown("Fire1")) {
                if (weaponManager.IsDualWielding()) {
                    InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate / 2);
                }
                else {
                    InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
                }
            } else if (Input.GetButtonUp("Fire1")) {
                CancelInvoke("Shoot");
            }
            currentBurst = 0;
        }

        if (!weaponManager.IsMelee() && !daulGun) {
            if ((shooting < currentWeapon.shootCooldown / 6 && !weaponManager.IsDualWielding()) || (shooting < currentWeapon.shootCooldown / 12)) {
                weaponHolder.transform.Rotate(-2, 0, 0 * Time.deltaTime);
                altWeaponHolder.transform.Rotate(2, 0, 0 * Time.deltaTime);
            }
            else if (shooting < currentWeapon.shootCooldown) {
                weaponHolder.transform.Rotate(0.33333333333f, 0, 0 * Time.deltaTime);
                altWeaponHolder.transform.Rotate(-0.33333333333f, 0, 0 * Time.deltaTime);
            }
            else if (!weaponManager.Swapping() && !weaponManager.IsReloading()) {
                weaponHolder.transform.rotation = cam.transform.rotation;
                altWeaponHolder.transform.rotation = weaponHolder.transform.rotation;
            }
        } else if (!weaponManager.IsMelee()) {
            if ((shooting < currentWeapon.shootCooldown / 6 && !weaponManager.IsDualWielding()) || (shooting < currentWeapon.shootCooldown / 12))
            {
                altWeaponHolder.transform.Rotate(-2, 0, 0 * Time.deltaTime);
            }
            else if (shooting < currentWeapon.shootCooldown)
            {
                altWeaponHolder.transform.Rotate(-0.33333333333f, 0, 0 * Time.deltaTime);
            }
            else if (!weaponManager.Swapping() && !weaponManager.IsReloading()) {
                weaponHolder.transform.rotation = cam.transform.rotation;
                altWeaponHolder.transform.rotation = cam.transform.rotation;
            }
        }

        shooting += 1;

        if (weaponManager.Swapping()) {
            shootCooldown = currentWeapon.shootOffset;
        }

    }

    [Command]
    void CmdOnShoot(Vector3 _pos, Vector3 _port, Quaternion _rot, int _barrel, bool _alt) {
        RpcDoShootEffect(_pos, _barrel, _alt);
        if (_port != Vector3.zero) {
            RpcDoCasingEffect(_port, _rot);
        }
    }

    [ClientRpc]
    void RpcDoShootEffect(Vector3 _pos, int _barrel, bool _alt) {

        if (!weaponManager.IsMelee() && !_alt)  {
            weaponManager.GetCurrentFirePoint().transform.GetChild(_barrel).GetComponent<ParticleSystem>().Play();
        } else if (!weaponManager.IsMelee()) {
            weaponManager.GetAltCurrentFirePoint().transform.GetChild(_barrel).GetComponent<ParticleSystem>().Play();
        }
        
        AudioSource _shootSound = (AudioSource)Instantiate(
            weaponManager.GetcurrentShootSound().GetComponent<AudioSource>(),
            _pos,
            new Quaternion(0, 0, 0, 0)
        );
        _shootSound.Play();
        Destroy(_shootSound.gameObject, weaponManager.GetcurrentShootSound().GetComponent<AudioSource>().clip.length);
    }

    [ClientRpc]
    void RpcDoCasingEffect(Vector3 _pos, Quaternion _rot) {
        GameObject _casing = (GameObject)Instantiate(weaponManager.GetCurrentCasing(), _pos, Random.rotation);
        _casing.GetComponent<Rigidbody>().velocity = _rot * new Vector3(1f, 0.3f, 0f) * Random.Range(5f, 10f);
        Destroy(_casing, 2f);
    }

    [Command]
    void CmdOnHit (Vector3 _pos, Vector3 _normal) {
        RpcDoHitEffect(_pos, _normal);
    }

    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal) {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    void Shoot() {

        if (currentBurst == 0 && weaponManager.IsDualWielding()) {
            daulGun = !daulGun;
            if (daulGun) {
                barrel += 1;
            }
        } else {
            barrel += 1;
        }

        if (!isLocalPlayer || !weaponManager.CanShoot()) {
            return;
        }

        weaponManager.Shooting();

        PlayerRB.AddForce(-(Vector3.RotateTowards(cam.transform.forward, Vector3.down, 0, 0)) * currentWeapon.force);

        if (barrel > currentWeapon.barrels - 1) {
            barrel = 0;
        }

        if (weaponManager.GetCurrentEjectionPort() != null && shooting > 0) {
            CmdOnShoot(cam.transform.position, weaponManager.GetCurrentEjectionPort().transform.position, transform.rotation, barrel, daulGun);
        } else {
            CmdOnShoot(cam.transform.position, Vector3.zero, transform.rotation, barrel, daulGun);
        }

        float _devience;

        if (!motor.IsGrounded()) {
            _devience = currentWeapon.spreadWhileJumping;
        } else if (motor.IsMoving() || weaponManager.IsDualWielding()) {  
            _devience = currentWeapon.spreadWhileMoving;
        } else {
            _devience = currentWeapon.spread;
        }

        Random.seed = System.DateTime.Now.Millisecond;

        for (int i = 0; i < currentWeapon.roundsPerShot; i++)  {
            if (currentWeapon.projectileWeapon) {

                Vector3 _spread = new Vector3(
                    Random.Range(-_devience, _devience),
                    Random.Range(-_devience, _devience),
                    Random.Range(-_devience, _devience)
                );

                Quaternion _rot = Quaternion.Euler(
                    Random.Range(-_devience, _devience) * 100, 
                    Random.Range(-_devience, _devience) * 100, 
                    Random.Range(-_devience, _devience) * 100
                );

                CmdProjectileShot(transform.position, cam.transform.rotation * _rot, (cam.transform.forward + _spread) * currentWeapon.throwPower, transform.name);

        } else {

            

                Vector3 _spread = new Vector3(
                    Random.Range(-_devience, _devience),
                    Random.Range(-_devience, _devience),
                    0
                );

                RaycastHit _hit;

                if (Physics.Raycast(cam.transform.position, cam.transform.forward + _spread, out _hit, currentWeapon.range, mask)) {
                    int _damage = Mathf.RoundToInt(currentWeapon.damageFallOff.Evaluate(_hit.distance / currentWeapon.range) * currentWeapon.damage);

                    if (_hit.collider.tag == "Player") {

                        string[] crits = { "Skull", "RightEye", "LeftEye" };
                        if (crits.Contains(_hit.collider.name)) {
                            _damage *= 3;
                        }

                        CmdPlayerShot(_hit.collider.transform.root.name, _damage, transform.name);
                    }

                    CmdOnHit(_hit.point, _hit.normal);

                    if (_hit.collider.GetComponent<Meshinator>() != null)  {
                        _hit.collider.GetComponent<Meshinator>().Impact(_hit.point, _hit.normal * -0.5f * _damage, Meshinator.ImpactShapes.SphericalImpact, Meshinator.ImpactTypes.Compression);
                    } 
                }

            }
        }

        shooting = 0;
        currentBurst += 1;
    }

    [Command]
    void CmdPlayerShot (string _playerID, int _damage, string _shooterID) {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _shooterID);
    }

    public void playerShot(string _playerID, int _damage, string _shooterID) {
        CmdPlayerShot(_playerID, _damage, _shooterID);
    }

    [Command]
    void CmdProjectileShot(Vector3 _pos, Quaternion _rot, Vector3 _vel, string _playerID) {
        GameObject _projectile = (GameObject)Instantiate(weaponManager.GetCurrentProjectile(), _pos, _rot);
        _projectile.GetComponent<Rigidbody>().velocity = _vel;
        _projectile.GetComponent<ProjectileController>().playerID = _playerID;
        NetworkServer.Spawn(_projectile);
    }
}
