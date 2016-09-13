using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour {

    public Collider collider;

    public GameObject impact;

    public GameObject explosionSound;

    public PlayerShoot shoot;

    public float damage;

    public float range;

    public bool repeats;

    public float repeatCooldown;

    public int bounces;

    public Rigidbody rb;

    public bool exploding = false;

    public bool explosive;

    public bool impacts;

    public bool sticky;

    public bool homing;

    public float homingness;

    public float homingnessInterval;

    public bool chain;

    public int armTime;

    public int life = 200;

    private int framesSinceCreated = 0;

    private Vector3 startPos;

    private Quaternion startRot;

    public string playerID;

    public float distance = Mathf.Infinity;
    public float diff;
    public Transform target;

    public bool renderers;

    private Vector3 lastPos;

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
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player")) {
                diff = (go.transform.position - transform.position).sqrMagnitude;

                if (diff < distance && go.transform.root.name != playerID && Vector3.Angle(transform.forward, go.transform.position - transform.position) < 15) {
                    distance = diff;
                    target = go.transform;
                }

            }

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Decoy")) {
                diff = (go.transform.position - transform.position).sqrMagnitude;

                if (diff / 2 < distance && go.transform.root.name != playerID && Vector3.Angle(transform.forward, go.transform.position - transform.position) < 60) {
                    distance = diff;
                    target = go.transform;
                }

            }

            if (target != null) {
                InvokeRepeating("Homing", 0f, homingnessInterval);
            }
        }

        

        SetRenderers(transform, false);
    }

    // Update is called once per frame
    void FixedUpdate() {
        framesSinceCreated += 1;

        if (Vector3.Distance(transform.position, startPos) > 0.5f && collider != null) {
            collider.enabled = true;
            SetRenderers(transform, true);
        }

        if (!explosive && bounces > 0) {
            transform.rotation = startRot;
        }

        if (!impacts && framesSinceCreated > life) {
            if (explosive) {
                CmdExplode(Quaternion.LookRotation(Vector3.up), chain);
            }
            Destroy(gameObject);
        } else if (sticky && framesSinceCreated > life) {
            Destroy(gameObject);
            CancelInvoke("SpawnRepeat");
        }
        else if (impacts && framesSinceCreated > life && !transform.root.name.Contains("Player")) {
            Destroy(gameObject);
        }

        lastPos = transform.position;
    }

    void Homing() {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, homingness));
            rb.velocity = transform.forward * rb.velocity.magnitude;



        if (Vector3.Distance(transform.position, target.transform.position) < 3 && explosive){
            CmdExplode(new Quaternion(270, 0, 0, 0), chain);
        }
    } 

    void OnCollisionEnter(Collision collision) {

        if (collision.collider.tag == "Shield") {
            playerID = collision.collider.transform.root.name;
            target = null;
            transform.Rotate(0, 180, 0);
        } else if (collision.collider.tag != "Projectile" && framesSinceCreated >= armTime) {
            bounces -= 1;
        }

        if (bounces < 1) {
            if (sticky && impacts) {
                Hit(collision);
                Destroy(gameObject, Time.deltaTime);
            } else if (explosive) {
                CmdExplode(Quaternion.LookRotation(collision.contacts[0].normal), chain);
            } else if (sticky) {
                Stick(collision);
            } else {
                Hit(collision);
            }
        }
    }

    [Command]
    void CmdExplode(Quaternion _rot, bool _chain) {
        RpcExplode(_rot, _chain);
    }

    [ClientRpc]
    public void RpcExplode(Quaternion _rot, bool _chain) {
        if (exploding) {
            return;
        }

        exploding = true;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);

        foreach (var _hit in hitColliders) {

            if (_hit.transform.name == "Skull" && _hit.transform.root.GetComponent<Player>()) {

                float _dist = Vector3.Distance(_hit.transform.position, gameObject.transform.position);

                _hit.transform.root.GetComponent<Player>().RpcTakeDamage(Mathf.RoundToInt(Mathf.Pow(range - _dist, 2) * damage), playerID);

            }

            if (_hit.transform.root.GetComponent<ProjectileController>() && _chain) {
                _hit.transform.root.GetComponent<ProjectileController>().playerID = playerID;
                _hit.transform.root.GetComponent<ProjectileController>().CmdExplode(Quaternion.LookRotation(Vector3.up), false);
            }
        }

        CmdImpactEffect(transform.position, _rot);
        Destroy(gameObject, 2f);      
    }

    public void Hit(Collision _collision) {
        if ((_collision.collider.tag == "Player") && _collision.collider.transform.root != _collision.collider.transform && _collision.collider.transform.root.name != playerID && Vector3.Distance(transform.position, lastPos) > 0.1f) {

            if (rb.mass > 3) {
                _collision.collider.transform.root.GetComponent<Rigidbody>().mass += rb.mass / 10;
            }

            Destroy(collider);
            Destroy(rb);
            transform.position = _collision.collider.transform.position;
            transform.SetParent(_collision.collider.transform);
            if (_collision.collider.name == "Skull") {
                _collision.transform.root.GetComponent<Player>().RpcTakeDamage(Mathf.RoundToInt(damage * 3), playerID);
            } else {
                _collision.transform.root.GetComponent<Player>().RpcTakeDamage(Mathf.RoundToInt(damage), playerID);
            }

            foreach (Transform child in transform) {
                if (child.GetComponent<TrailRenderer>() != null)
                {
                    child.GetComponent<TrailRenderer>().enabled = false;
                }
            }
        }

        CmdImpactEffect(transform.position, Quaternion.LookRotation(_collision.contacts[0].normal));
        GetComponent<NetworkTransform>().enabled = false;
    }

    public void Stick(Collision _collision) {

        if (_collision.collider.tag != "Projectile") {
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;


            CmdImpactEffect(transform.position, Quaternion.LookRotation(_collision.contacts[0].normal));
        }
    }

    void SpawnRepeat() {
        GameObject _impact = (GameObject)Instantiate(impact, transform.position, Quaternion.LookRotation(Vector3.up));

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

    public void SetRenderers(Transform _obj, bool _state) {
        if (renderers) {
            foreach (Transform child in _obj) {
                if (child.GetComponent<ParticleSystem>() == null) {
                    SetRenderers(child.transform, _state);
                    child.GetComponent<Renderer>().enabled = _state;
                }
            }
        }
    }

    void OnDrawGizmosSelected () {
        if (explosive) {
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }

    [Command]
    void CmdImpactEffect(Vector3 _pos, Quaternion _rot) {
        RpcDoImpactEffect(_pos, _rot);
    }

    [ClientRpc]
    void RpcDoImpactEffect(Vector3 _pos, Quaternion _rot) {
        GameObject _impact = (GameObject)Instantiate(impact, _pos, _rot);
        float time = 0;

        if (_impact.GetComponent<ParticleSystem>() == null) {
            time = _impact.transform.GetChild(0).GetComponent<ParticleSystem>().duration;
        } else {
            time = _impact.GetComponent<ParticleSystem>().duration;
        }

        Destroy(_impact, time);

        if (explosive) {
            AudioSource _explosionSound = (AudioSource)Instantiate(
                explosionSound.GetComponent<AudioSource>(),
                transform.position,
                new Quaternion(0, 0, 0, 0)
            );
            _explosionSound.Play();
            Destroy(_explosionSound.gameObject, 5f);
        }
    }
}
