using UnityEngine;
using UnityEngine.Networking;


public class RenderCamera : NetworkBehaviour   {

    private Texture2D renderedTexture;

    public Material mat;

    void Start() {
        renderedTexture = new Texture2D(Screen.width / 10, Screen.height / 10);
        mat.mainTexture = renderedTexture;
    }

    void OnPostRender () {

        renderedTexture.ReadPixels(new Rect(Screen.width / Mathf.Sqrt(5), Screen.height / Mathf.Sqrt(5), Screen.width / 10, Screen.height / 10), 0, 0);

        renderedTexture.Apply();
        
    }
}
