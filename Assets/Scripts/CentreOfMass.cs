using UnityEngine;
using System.Collections;

public class CentreOfMass : MonoBehaviour {
    public Vector3 centre;
    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.centerOfMass = centre;
        }
    }
}
