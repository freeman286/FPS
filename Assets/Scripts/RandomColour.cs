using UnityEngine;
using System.Collections;

public class RandomColour : MonoBehaviour {

	// Use this for initialization
	void Start () {
        float r = 0;
        float g = 0;
        float b = 0;


        Random.seed = System.DateTime.Now.Millisecond;
        while (!((r < 0.3f || g < 0.3f || b < 0.3f) && (r > 0.7f || g > 0.7f || b > 0.7f)))
        {
            r = Random.Range(0.1f, 1.0f);
            b = Random.Range(0.1f, 1.0f);
            g = Random.Range(0.1f, 1.0f);
        }
        Color _color = new Color(r, g, b);

        GetComponent<Renderer>().material.color = _color;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
