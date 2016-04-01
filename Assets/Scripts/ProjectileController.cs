using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    public Collider collider;

    public GameObject impact;

    public GameObject explosionSound;

    public PlayerShoot shoot;

    public int damage;

    public bool exploding = false;

    private int framesSinceCreated = 0;

    private Vector3 start;


    // Use this for initialization
    void Start() {
        collider.enabled = false;
        start = transform.position;
    }

    // Update is called once per frame
    void Update() {
        framesSinceCreated += 1;

        if (Vector3.Distance(transform.position, start) > 0.5f) {
            collider.enabled = true;
        }

        if (framesSinceCreated > 10000) {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision) {
        Explode(collision);
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
        Destroy(_explosionSound.gameObject, 1f);
    }
}
