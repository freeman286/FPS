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

        Debug.Log(Screen.width);
        Debug.Log(Screen.height);

        renderedTexture.ReadPixels(new Rect(Screen.width / 2.25f, Screen.height / 2.25f, Screen.width / 10, Screen.height / 10), 0, 0);

        renderedTexture.Apply();
        
    }
}
