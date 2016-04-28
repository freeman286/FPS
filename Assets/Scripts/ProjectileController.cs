using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    public Collider collider;

    public GameObject impact;

    public GameObject explosionSound;

    public PlayerShoot shoot;

    public float damage;

    public bool repeats;

    public float repeatCooldown;

    public int bounces;

    public Rigidbody rb;

    public bool exploding = false;

    public bool explosive;

    public bool impacts;

    public bool sticky;

    public bool homing;

    public int homingness;

    public int life = 200;

    private int framesSinceCreated = 0;

    private Vector3 startPos;

    private Quaternion startRot;

    public string playerID;

    public float distance = Mathf.Infinity;
    public float diff;
    public Transform target;


    // Use this for initialization
    void Start() {
        collider.enabled = false;
        startPos = transform.position;
        startRot = transform.rotation;
        if (repeats) {
            InvokeRepeating("SpawnRepeat", 0.1f, repeatCooldown);
            rb.isKinematic = false;
            rb.WakeUp();
        }
        if (homing) {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                diff = (go.transform.position - transform.position).sqrMagnitude;

                if (diff < distance && go.transform.root.name != playerID && Vector3.Angle(transform.forward, go.transform.position - transform.position) < 30)
                {
                    distance = diff;
                    target = go.transform;
                }

            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Decoy"))
            {
                diff = (go.transform.position - transform.position).sqrMagnitude;

                if (diff < distance && go.transform.root.name != playerID && Vector3.Angle(transform.forward, go.transform.position - transform.position) < 60)
                {
                    distance = diff;
                    target = go.transform;
                }

            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        framesSinceCreated += 1;

        if (Vector3.Distance(transform.position, startPos) > 0.5f && collider != null) {
            collider.enabled = true;
        }

        if (!explosive && bounces > 0) {
            transform.rotation = startRot;
        }

        if ((framesSinceCreated > life || (!explosive && !sticky && bounces < 1)) && !impacts) {
            Destroy(gameObject);
        } else if (sticky && framesSinceCreated > life) {
            Destroy(gameObject);
            CancelInvoke("SpawnRepeat");
        }
        else if (impacts && framesSinceCreated > life && !transform.root.name.Contains("Player")) {
            Destroy(gameObject);
        }

        if (homing && target != null && Time.fixedTime % 0.01 == 0) {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

            Debug.Log(target);

            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, homingness));

            rb.velocity = transform.forward * rb.velocity.magnitude;

           
        }

        if (homing && target != null && Vector3.Distance(transform.position, target.transform.position) < 3 && explosive) {
            exploding = true;
            GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.identity);
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
    }

    void OnCollisionEnter(Collision collision) {

        bounces -= 1;

        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 10f);

        if (bounces < 1) {
            if (explosive) {
                Explode(collision);
            } else if (sticky) {
                Stick(collision);
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

        if (_collision.collider.tag == "Weapon") {
            transform.localScale = new Vector3(0.1f / transform.parent.localScale.x, 0.1f / transform.parent.localScale.y, 0.05f / transform.parent.localScale.z);
        }

        GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.LookRotation(_collision.contacts[0].normal));

        float time = 0;

        if (_impact.GetComponent<ParticleSystem>() == null) {
            time = _impact.transform.GetChild(0).GetComponent<ParticleSystem>().duration;
        } else {
            time = _impact.GetComponent<ParticleSystem>().duration * 10;
        }

         Destroy(_impact, time); 
    }

    public void Stick(Collision _collision) {

        if (_collision.collider.tag != "Projectile") {
            rb.velocity = Vector3.zero;
            rb.drag = 100;

            GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.LookRotation(_collision.contacts[0].normal));

            float time = 0;

            if (_impact.GetComponent<ParticleSystem>() == null)
            {
                time = _impact.transform.GetChild(0).GetComponent<ParticleSystem>().duration;
            }
            else {
                time = _impact.GetComponent<ParticleSystem>().duration * 10;
            }

            Destroy(_impact, time);
        }
    }

    void SpawnRepeat() {
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.identity);

        float time = 0;

        if (_impact.GetComponent<ParticleSystem>() == null)
        {
            time = _impact.transform.GetChild(0).GetComponent<ParticleSystem>().duration;
        }
        else {
            time = _impact.GetComponent<ParticleSystem>().duration * 10;
        }

        Destroy(_impact, time);
    }
  }
