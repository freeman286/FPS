using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    public Collider collider;

    public GameObject impact;

    public GameObject explosionSound;

    public PlayerShoot shoot;

    public int damage;

    public int bounces;

    public Rigidbody rb;

    public bool exploding = false;

    public bool explosive;

    private int framesSinceCreated = 0;

    private Vector3 startPos;

    private Quaternion startRot;

    public string playerID;



    // Use this for initialization
    void Start() {
        collider.enabled = false;
        startPos = transform.position;
        startRot = transform.rotation;
    }

    // Update is called once per frame
    void Update() {
        framesSinceCreated += 1;

        if (Vector3.Distance(transform.position, startPos) > 0.5f) {
            collider.enabled = true;
        }

        if (!explosive && bounces > 0) {
            transform.rotation = startRot;
        }

        if (framesSinceCreated > 10000 || (!explosive && bounces < 1)) {
            Destroy(gameObject);
        }


    }

    void OnCollisionEnter(Collision collision) {

        bounces -= 1;

        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 10f);

        if (bounces < 1) {
            if (explosive) {
                Explode(collision);
            } else if (collision.collider.tag != "Projectile") {
                Hit(collision);
            }
        }
    }

    public void Explode (Collision _collision) {
        exploding = true;
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.LookRotation(_collision.contacts[0].normal));
        Destroy(_impact, 10f);
        Destroy(gameObject, Time.deltaTime);
        AudioSource _explosionSound = (AudioSource)Instantiate(
            explosionSound.GetComponent<AudioSource>(),
            transform.position,
            new Quaternion(0, 0, 0, 0)
        );
        _explosionSound.Play();
        Destroy(_explosionSound.gameObject, 5f);
    }

    public void Hit(Collision _collision) {
        if (_collision.collider.tag == "Player" || _collision.collider.tag == "Weapon") {
            Destroy(rb);
            transform.position = _collision.collider.transform.position;
            transform.SetParent(_collision.collider.transform);
        }
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.LookRotation(_collision.contacts[0].normal));
        Destroy(_impact, 20f);  
    }
}
