﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isDead = false;
    public bool isDead {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

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

    public void Setup () {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++) {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }

    void Update () {
        if (rb.position.y < -10) {
            Die();
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount) {
        if (isDead)
            return;

        currentHealth -= _amount;

        Debug.Log(transform.name + " now has " + currentHealth + " health.");

        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        //hello git

        isDead = true;

        shoot.CancelInvoke("Shoot");

        Destroy(playerUIInstance);

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

        Debug.Log(transform.name + " is Dead!");

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


        Debug.Log(transform.name + " Respawned");

    }

    public void SetDefaults() {
        isDead = false;

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
    }

}
