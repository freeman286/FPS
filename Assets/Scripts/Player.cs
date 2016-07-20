using UnityEngine;
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

    public string team = "";

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
    public float[] z;

    private Transform trans;

    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private GameObject weaponHolder;

    [SerializeField]
    private GameObject altWeaponHolder;

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

    public void ExteriorSetup() {

        float r = 0;
        float g = 0;
        float b = 0;

        if (GameManager.instance.matchSettings.gameMode == "Deathmatch") {
            Random.seed = int.Parse(Regex.Replace(transform.name, "[^0-9]", "")) * System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
            while (!((r < 0.3f || g < 0.3f || b < 0.3f) && (r > 0.7f || g > 0.7f || b > 0.7f)))
            {
                r = Random.Range(0.1f, 1.0f);
                b = Random.Range(0.1f, 1.0f);
                g = Random.Range(0.1f, 1.0f);
            }
        } else if (GameManager.instance.matchSettings.gameMode == "Team Deathmatch") {
            if (GameManager.GetPlayer(transform.name).team == "Red")  {
                r = 1f;
            } else {
                b = 1f;
            }
        }


        Color _color = new Color(r, g, b);

        for (int i = 0; i < rigidbodyOnDeath.Length - 1; i++)
        {
            if (rigidbodyOnDeath[i].GetComponent<Renderer>() != null)
            {
                rigidbodyOnDeath[i].GetComponent<Renderer>().material.color = _color;
            }
        }

        cam.transform.position = gameObject.transform.position;
        cam.transform.parent = gameObject.transform;
        weaponHolder.transform.parent = cam.transform;
        weaponHolder.transform.rotation = cam.transform.rotation;
        altWeaponHolder.transform.parent = weaponHolder.transform;

        weaponManager.FillMags();
        weaponManager.EquipPrimary();
        shoot.currentBurst = 0;
        controller.Reset();
    }

    public void PlayerSetup() {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++) {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        if (GameManager.instance.matchSettings.gameMode == "Team Deathmatch") {

            int _redCount = 0;
            int _blueCount = 0;

            foreach (string _playerID in GameManager.players.Keys)
            {
                if (GameManager.players[_playerID].team == "Red") {
                    _redCount += 1;
                } else {
                    _blueCount += 1;
                }


            }

            if (_redCount > _blueCount) {
                team = "Blue";
            } else {
                team = "Red";
            }
        }

        SetDefaults();
    }

    void Update () {
        if (rb.position.y < -10 && !isDead) {
            Die();
            GameManager.GetPlayer(lastDamage).kills += 1;
        }
        healthRegen += 1;
        if (healthRegen > 300 && currentHealth < maxHealth) {
            currentHealth += 1;
        }

        timeSinceSpawned += 1;
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _shooterID) {
        if (isDead || _amount < 0)
            return;

        if (_shooterID != transform.name) {
            lastDamage = _shooterID;
        }

        if (isLocalPlayer && _amount != 0) {
            AudioSource _dingSound = (AudioSource)Instantiate(
                ding.GetComponent<AudioSource>(),
                cam.transform.position,
                new Quaternion(0, 0, 0, 0)
            );
            _dingSound.Play();
            Destroy(_dingSound.gameObject, 1f);
        }

        if (timeSinceSpawned > 100 && !(GameManager.instance.matchSettings.gameMode == "Team Deathmatch" && GameManager.players[_shooterID].team == team)) {
            currentHealth -= _amount;
        }

        healthRegen = 0;

        if (currentHealth <= 0) {
            Die();
            GameManager.GetPlayer(lastDamage).kills += 1;
        }
    }

    private void Die() {

        deaths += 1;

        isDead = true;

        shoot.CancelInvoke("Shoot");

        Destroy(playerUIInstance);

        weaponHolder.transform.parent = transform;
        altWeaponHolder.transform.parent = transform;
        cam.transform.parent = rigidbodyOnDeath[0].transform;

        string[] _bodyPart = { "Torso", "Skull", "LeftFoot", "RightFoot", "WeaponHolder", "AltWeaponHolder" };
        float[] _mass = { 1f, 0.5f, 0.2f, 0.2f, 1, 1f };

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

        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        for (int i = 0; i < rigidbodyOnDeath.Length; i++) {
            Destroy(rigidbodyOnDeath[i].GetComponent<Rigidbody>());
        }

        for (int i = 0; i < rigidbodyOnDeath.Length; i++) {
            trans = rigidbodyOnDeath[i].GetComponent<Transform>();
            trans.position = _spawnPoint.position + new Vector3(x[i], y[i], z[i]);
            trans.rotation = _spawnPoint.rotation;
        }

        SetDefaults();

    }

    public void SetDefaults() {
        isDead = false;

        if (timeSinceSpawned > 0) {
            timeSinceSpawned = 0;
        }
         
        rb.velocity = Vector3.zero;
        rb.mass = 0.5f;

        cam.transform.position = gameObject.transform.position;
        cam.transform.parent = gameObject.transform;
        weaponHolder.transform.parent = cam.transform;
        weaponHolder.transform.rotation = cam.transform.rotation;
        altWeaponHolder.transform.parent = weaponHolder.transform;

        // Overide altWeaponHolder position
        altWeaponHolder.transform.localPosition = new Vector3(-0.912f, 0, 0);

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
