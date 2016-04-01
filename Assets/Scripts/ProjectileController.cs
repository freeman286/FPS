using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    public Collider collider;

    public GameObject impact;

    public GameObject explosionSound;

    public PlayerShoot shoot;

    public bool exploding = false;

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
