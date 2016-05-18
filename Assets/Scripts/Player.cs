﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isDead = false;
    public bool isDead {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    public bool isDamaged {
        get { return currentHealth != maxHealth; }
    }

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    [SyncVar]
    public string lastDamage;

    [SyncVar]
    private int healthRegen = 0;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    public GameObject[] rigidbodyOnDeath;
    public float[] x;
    public float[] y;

    private Transform trans;

    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    GameObject playerUIPrefab;
    private GameObject playerUIInstance;

    [SerializeField]
    private PlayerShoot shoot;

    public WeaponManager weaponManager;

    [SerializeField]
    private PlayerController controller;

    public GameObject ding;

    [SyncVar]
    public int kills = 0;

    [SyncVar]
    public int deaths = 0;

    [SyncVar]
    public int timeSinceSpawned = -1;

    public void Setup () {

        float r = 0;
        float g = 0;
        float b = 0;

        Random.seed = int.Parse(Regex.Replace(transform.name, "[^0-9]", "")) * System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        while (!((r < 0.3f || g < 0.3f || b < 0.3f) && (r > 0.7f || g > 0.7f || b > 0.7f))) {
            r = Random.Range(0.1f, 1.0f);
            b = Random.Range(0.1f, 1.0f);
            g = Random.Range(0.1f, 1.0f);
        }
        Color _color = new Color(r, g, b);

        for (int i = 0; i < rigidbodyOnDeath.Length - 1; i++) {
            rigidbodyOnDeath[i].GetComponent<Renderer>().material.color = _color;
        }

        

        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++) {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }

    void Update () {
        if (rb.position.y < -10 && !isDead) {
            Die();
            if (lastDamage != transform.name) {
                GameManager.GetPlayer(lastDamage).kills += 1;
            }
        }
        healthRegen += 1;
        if (healthRegen > 300 && currentHealth < maxHealth) {
            currentHealth += 1;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10);
        foreach (var _hit in hitColliders) {
            if (_hit.transform.root.tag == "Projectile" || _hit.transform.root.tag == "Decoy")  {
                if (_hit.transform.root.GetComponent<ProjectileController>().exploding || ! _hit.transform.root.GetComponent<ProjectileController>().explosive) {

                    float _dist = Vector3.Distance(_hit.transform.position, gameObject.transform.position);

                    if (_hit.transform.root.GetComponent<ProjectileController>().explosive) {
                        RpcTakeDamage(Mathf.RoundToInt(Mathf.Pow(10 - _dist, 2) * _hit.transform.root.GetComponent<ProjectileController>().damage), _hit.transform.root.GetComponent<ProjectileController>().playerID);
                    } else if (_dist < 1.5f) {
                        RpcTakeDamage(Mathf.RoundToInt(Mathf.Pow(3 - _dist, 2) * _hit.transform.root.GetComponent<ProjectileController>().damage), _hit.transform.root.GetComponent<ProjectileController>().playerID);
                    }

                }
            }
        }

        timeSinceSpawned += 1;
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _shooterID) {
        if (isDead || _amount < 0)
            return;

        lastDamage = _shooterID;

        if (isLocalPlayer && _amount != 0) {
            AudioSource _dingSound = (AudioSource)Instantiate(
                ding.GetComponent<AudioSource>(),
                cam.transform.position,
                new Quaternion(0, 0, 0, 0)
            );
            _dingSound.Play();
            Destroy(_dingSound.gameObject, 1f);
        }

        if (timeSinceSpawned > 100) {
            currentHealth -= _amount;
        }

        healthRegen = 0;

        if (currentHealth <= 0) {
            Die();
            if (_shooterID != transform.name) {
                GameManager.GetPlayer(_shooterID).kills += 1;
            }
        }
    }

    private void Die() {

        deaths += 1;

        isDead = true;

        if (weaponManager.IsDualWielding()) {
            weaponManager.StopDualWielding();
        }

        shoot.CancelInvoke("Shoot");

        Destroy(playerUIInstance);

        cam.transform.parent = rigidbodyOnDeath[0].transform;

        RemoveProjectilesRecursively(cam.transform);

        string[] _bodyPart = { "Torso", "Skull", "LeftFoot", "RightFoot", "WeaponHolder" };
        float[] _mass = { 3f, 2f, 2f, 2f, 2f };

        for (int i = 0; i < rigidbodyOnDeath.Length; i++) {
            Rigidbody rigidbody = rigidbodyOnDeath[i].AddComponent<Rigidbody>();
            rigidbody.mass = _mass[System.Array.IndexOf(_bodyPart, rigidbodyOnDeath[i].name)];
            rigidbody.AddForce(-Vector3.forward * rigidbody.mass * 100);
        }

        for (int i = 0; i < disableOnDeath.Length; i++) {
            disableOnDeath[i].enabled = false;
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null) {
            _col.enabled = false;
        }

        StartCoroutine(Respawn());

    }

    private IEnumerator Respawn () {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        SetDefaults();
        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        for (int i = 0; i < rigidbodyOnDeath.Length; i++)
        {
            trans = rigidbodyOnDeath[i].GetComponent<Transform>();
            trans.position = _spawnPoint.position + new Vector3(x[i], y[i], 0);
            trans.rotation = _spawnPoint.rotation;
        }

        rigidbodyOnDeath[4].transform.rotation = cam.transform.rotation;

    }

    public void SetDefaults() {
        isDead = false;

        if (timeSinceSpawned > 0) {
            timeSinceSpawned = 0;
        }

        lastDamage = transform.name;
         
        rb.velocity = Vector3.zero;
        rb.mass = 0.5f;

        cam.transform.position = gameObject.transform.position;
        cam.transform.parent = gameObject.transform;

        weaponManager.FillMags();
        weaponManager.EquipPrimary();
        shoot.currentBurst = 0;
        controller.Reset();

        currentHealth = maxHealth;

        for (int i = 0; i < disableOnDeath.Length; i++) {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        for (int i = 0; i < rigidbodyOnDeath.Length; i++) {  
            Destroy(rigidbodyOnDeath[i].GetComponent<Rigidbody>());
        }


        Collider _col = GetComponent<Collider>();
        if (_col != null) {
            _col.enabled = true;
        }

        playerUIInstance = Instantiate(playerUIPrefab);
        playerUIInstance.name = playerUIPrefab.name;

        RemoveProjectilesRecursively(transform);

    }

    public void RemoveProjectilesRecursively(Transform _obj) {
        foreach (Transform child in _obj) {
            RemoveProjectilesRecursively(child.transform);
            if (child.tag == "Projectile") {
                Destroy(child.gameObject);
            }
        }
    }

}
