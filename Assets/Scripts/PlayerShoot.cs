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

    private int shootCooldown = 200;

    [SerializeField]
    private PlayerMotor motor;

    [SerializeField]
    private Transform weaponHolder;

    private int shooting = 100;

    public int currentBurst = 0;

    void Start() {
        if (cam == null)
        {
            Debug.Log("No camera referenced");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
    }

    void Update() {
                
        currentWeapon = weaponManager.GetCurrentWeapon();
        shootCooldown += 1;
        if (currentWeapon.fireRate <= 0 && shootCooldown > currentWeapon.shootCooldown) {
            if (Input.GetButtonDown("Fire1")) {
                Shoot();
                shootCooldown = 0;
                currentBurst = 0;
            }
        } else if (currentWeapon.burst > 1) {
            if (Input.GetButtonDown("Fire1") && currentBurst == 0) {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }

            if (currentBurst >= currentWeapon.burst) {
                CancelInvoke("Shoot");
                currentBurst = 0;
            }
        } else if (currentWeapon.fireRate > 0) {
            if (Input.GetButtonDown("Fire1")) {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1")) {
                CancelInvoke("Shoot");
            }
            currentBurst = 0;
        }

        if (!weaponManager.IsMelee()) {
            if (shooting < currentWeapon.shootCooldown / 6)
            {
                weaponHolder.transform.Rotate(-2, 0, 0 * Time.deltaTime);
            }
            else if (shooting < currentWeapon.shootCooldown)
            {
                weaponHolder.transform.Rotate(0.33333333333f, 0, 0 * Time.deltaTime);
            }
            else if (!weaponManager.Swapping() && !weaponManager.IsReloading())
            {
                weaponHolder.transform.rotation = cam.transform.rotation;
            }
        }

        shooting += 1;

    }

    [Command]
    void CmdOnShoot (Vector3 _pos, Vector3 _port, Quaternion _rot) {
        RpcDoShootEffect(_pos);
        if (_port != Vector3.zero) {
            RpcDoCasingEffect(_port, _rot);
        }
    }

    [ClientRpc]
    void RpcDoShootEffect(Vector3 _pos) {

        if (!weaponManager.IsMelee())  {
            weaponManager.GetCurrentFirePoint().GetComponentInChildren<ParticleSystem>().Play();
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


        if (!isLocalPlayer || !weaponManager.CanShoot()) {
            return;
        }

        weaponManager.Shooting();

        PlayerRB.AddForce(-(Vector3.RotateTowards(cam.transform.forward, Vector3.down, 1, 1)) * currentWeapon.force);

        shooting = 0;
        currentBurst += 1;


        if (weaponManager.GetCurrentEjectionPort() != null) {
            CmdOnShoot(cam.transform.position, weaponManager.GetCurrentEjectionPort().transform.position, transform.rotation);
        } else {
            CmdOnShoot(cam.transform.position, Vector3.zero, transform.rotation);
        }

        float _devience;

        if (!motor.IsGrounded()) {
            _devience = currentWeapon.spreadWhileJumping;
        } else if (motor.IsMoving()) {  
            _devience = currentWeapon.spreadWhileMoving;
        } else {
            _devience = currentWeapon.spread;
        }

        if (currentWeapon.projectileWeapon) {

            Vector3 _spread = new Vector3(
                Random.Range(-_devience, _devience),
                Random.Range(-_devience, _devience),
                0
            );

            CmdProjectileShot(transform.position, cam.transform.rotation, (cam.transform.forward + _spread) * currentWeapon.throwPower, transform.name);

        } else {

            for (int i = 0; i < currentWeapon.roundsPerShot; i++) {

                Vector3 _spread = new Vector3(
                    Random.Range(-_devience, _devience),
                    Random.Range(-_devience, _devience),
                    0
                );

                RaycastHit _hit;

                if (Physics.Raycast(cam.transform.position, cam.transform.forward + _spread, out _hit, currentWeapon.range, mask))
                {
                    if (_hit.collider.tag == "Player")
                    {

                        string[] crits = { "Skull", "RightEye", "LeftEye" };

                        int _damage = currentWeapon.damage;

                        if (crits.Contains(_hit.collider.name)) {
                            _damage *= 3;
                        }

                        CmdPlayerShot(_hit.collider.transform.root.name, _damage, transform.name);
                    }

                    CmdOnHit(_hit.point, _hit.normal);
                }

            }
        }

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
        RpcProjectileShot(_pos, _rot, _vel, _playerID);
    }

    [ClientRpc]
    void RpcProjectileShot(Vector3 _pos, Quaternion _rot, Vector3 _vel, string _playerID) {
        GameObject _projectile = (GameObject)Instantiate(weaponManager.GetCurrentProjectile(), _pos, _rot);
        _projectile.GetComponent<Rigidbody>().velocity = _vel;
        _projectile.GetComponent<ProjectileController>().playerID = _playerID;
    }
}
