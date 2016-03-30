using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    public Collider collider;

    public GameObject impact;

    public PlayerShoot shoot;

    private int framesSinceCreated = 0;


    // Use this for initialization
    void Start() {
        collider.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        framesSinceCreated += 1;

        if (framesSinceCreated > 0) {
            collider.enabled = true;
        }

        if (framesSinceCreated > 10000) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision) {
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.identity);
        Destroy(_impact, 10f);
        Destroy(gameObject);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5);
        foreach (var _hit in hitColliders)
        {
            if (_hit.transform.root.name.Contains("Player")) {
                Debug.Log(_hit.transform.root.name);

                CmdDamagedShot(_hit.transform.root.name, 100);
            }
        }
    }

    void CmdDamagedShot(string _playerID, int _damage)
    {
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }

}
