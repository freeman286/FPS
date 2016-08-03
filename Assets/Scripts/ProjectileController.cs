﻿using UnityEngine;
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

        if ((framesSinceCreated > life || (!explosive && !sticky && bounces < 1)) && !impacts) {
            if (explosive) {
                Explode(Quaternion.identity, true);
            }
            Destroy(gameObject);
        } else if (sticky && framesSinceCreated > life) {
            Destroy(gameObject);
            CancelInvoke("SpawnRepeat");
        }
        else if (impacts && framesSinceCreated > life && !transform.root.name.Contains("Player")) {
            Destroy(gameObject);
        }

        if (System.DateTime.Now.Millisecond % 20 == 0 && homing && target != null && Vector3.Distance(transform.position, target.transform.position) < 3 && explosive) {
            Explode(Quaternion.identity, true);
        }

        lastPos = transform.position;
    }

    void OnCollisionEnter(Collision collision) {

        if (collision.collider.tag != "Shield") {
            bounces -= 1;
        }
        else {
            playerID = collision.collider.transform.root.name;
            target = null;
            transform.Rotate(0, 180, 0);
        }

        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 10f);

        if (bounces < 1) {
            if (explosive) {
                Explode(Quaternion.LookRotation(collision.contacts[0].normal), true);
            } else if (sticky) {
                Stick(collision);
            } else if (collision.collider.tag != "Projectile") {
                Hit(collision);
            }
        }
    }

    public void Explode (Quaternion _rot, bool _chain) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10);

        foreach (var _hit in hitColliders) {

            if (_hit.transform.name == "Skull" && _hit.transform.root.GetComponent<Player>()) {

                float _dist = Vector3.Distance(_hit.transform.position, gameObject.transform.position);

                if (range - _dist > 0) {
                    _hit.transform.root.GetComponent<Player>().RpcTakeDamage(Mathf.RoundToInt(Mathf.Pow(range - _dist, 2) * damage), playerID);
                }

            }

            if (_hit.transform.root.GetComponent<ProjectileController>() && _chain) {
                _hit.transform.root.GetComponent<ProjectileController>().playerID = playerID;
                _hit.transform.root.GetComponent<ProjectileController>().Explode(Quaternion.identity, false);
            }
        }


        GameObject _impact = (GameObject)Instantiate(impact, transform.position, _rot);
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
}
