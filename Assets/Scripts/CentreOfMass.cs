using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CentreOfMass : MonoBehaviour {
    public Vector3 centre;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centre;
    }
}
