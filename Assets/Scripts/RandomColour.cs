using UnityEngine;
using System.Collections;

public class RandomColour : MonoBehaviour {
	void Start () {
        float r = 0;
        float g = 0;
        float b = 0;


        Random.seed = System.DateTime.Now.Millisecond;
        while (!((r < 0.1f || g < 0.1f || b < 0.1f) && (r > 0.3f || g > 0.3f || b > 0.3f)))
        {
            r = Random.Range(0f, 0.4f);
            b = Random.Range(0f, 0.4f);
            g = Random.Range(0f, 0.4f);
        }
        Color _color = new Color(r, g, b);

        GetComponent<Renderer>().material.color = _color;
    }
}
