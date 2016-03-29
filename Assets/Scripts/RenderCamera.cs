using UnityEngine;
using UnityEngine.Networking;


public class RenderCamera : NetworkBehaviour   {

    private Texture2D renderedTexture;

    public Material mat;

    void Awake() {
        renderedTexture = new Texture2D(Screen.width, Screen.height);
        mat.mainTexture = renderedTexture;
    }

    void OnPostRender () {
        if (Time.frameCount % 4 == 0) {
            renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

            renderedTexture.Apply();
        }

        
    }
}
