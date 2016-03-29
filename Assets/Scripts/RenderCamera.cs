using UnityEngine;
using UnityEngine.Networking;


public class RenderCamera : NetworkBehaviour   {

    private Texture2D renderedTexture;

    public Material mat;

    public WeaponManager weaponManager;

    void Start() {
        if (weaponManager.GetCurrentWeapon().name == "sniper") {
            renderedTexture = new Texture2D(Screen.width, Screen.height);
            mat.mainTexture = renderedTexture;
        }
    }

    void OnPostRender () {

        if (weaponManager.GetCurrentWeapon().name == "sniper") {

            renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

            renderedTexture.Apply();

        }
        
    }
}
