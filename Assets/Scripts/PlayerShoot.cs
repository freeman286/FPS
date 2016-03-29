﻿using UnityEngine;
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

    private GameObject shootSound;

    private int shootCooldown = 100;

    [SerializeField]
    private PlayerMotor motor;

    [SerializeField]
    private Transform weaponHolder;

    private int shooting = 100;

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
            }
        }
        else if (currentWeapon.fireRate > 0) {
            if (Input.GetButtonDown("Fire1")) {
                InvokeRepeating("Shoot", 0f, 1f / currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1")) {
                CancelInvoke("Shoot");
            }
        }

        if (shooting < currentWeapon.shootCooldown / 6) {
            weaponHolder.transform.Rotate(-2, 0, 0 * Time.deltaTime);
        } else if (shooting < currentWeapon.shootCooldown) {
            weaponHolder.transform.Rotate(0.33333333333f, 0, 0 * Time.deltaTime);
        } else if (!weaponManager.Swapping() && !weaponManager.IsReloading()) {
            weaponHolder.transform.rotation = cam.transform.rotation;
        }

        shooting += 1;
    }

    [Command]
    void CmdOnShoot () {
        RpcDoShootEffect();
    }

    [ClientRpc]
    void RpcDoShootEffect () {
        weaponManager.GetCurrentFirePoint().GetComponentInChildren<ParticleSystem>().Play();
        AudioSource _shootSound = (AudioSource)Instantiate(
            weaponManager.GetcurrentShootSound().GetComponent<AudioSource>(),
            cam.transform.position,
            new Quaternion(0, 0, 0, 0)
        );
        _shootSound.Play();
        Destroy(_shootSound.gameObject, 1f);
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


        if (!isLocalPlayer || !weaponManager.CanShoot() || weaponManager.IsReloading()) {
            return;
        }

        weaponManager.Shooting();

        shooting = 0;

        CmdOnShoot();

        float _devience;

        if (!motor.IsGrounded()) {
            _devience = currentWeapon.spreadWhileJumping;
        } else if (motor.IsMoving()) {
            _devience = currentWeapon.spreadWhileMoving;
        } else {
            _devience = currentWeapon.spread;
        }


        for (int i = 0; i < currentWeapon.roundsPerShot; i++) {
            RaycastHit _hit;
            Vector3 _spread = new Vector3(
                Random.Range(-_devience, _devience),
                Random.Range(-_devience, _devience),
                0);
            if (Physics.Raycast(cam.transform.position, cam.transform.forward + _spread, out _hit, currentWeapon.range, mask))
            {
                if (_hit.collider.tag == "Player")
                {

                    string[] crits = { "Skull", "RightEye", "LeftEye" };

                    int _damage = currentWeapon.damage;

                    if (crits.Contains(_hit.collider.name))
                    {
                        _damage *= 3;
                    }

                    CmdPlayerShot(_hit.collider.transform.root.name, _damage);
                }

                rb = _hit.collider.gameObject.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(transform.forward * 500f);
                }

                CmdOnHit(_hit.point, _hit.normal);
            }

        
        }

    }

    [Command]
    void CmdPlayerShot (string _playerID, int _damage) {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}
