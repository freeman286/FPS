using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] ComponentsToDisable;

    Camera sceneCamera;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";
    [SerializeField]
    GameObject playerGraphics;

    


    void Start () {
        if (!isLocalPlayer) {
            DisableComponents();
            AssignRemoteLayer();
        }
        else {
            sceneCamera = Camera.main;
            if (sceneCamera != null) {
                sceneCamera.gameObject.SetActive(false);
            }

            SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));
        }

        GetComponent<Player>().Setup();
    }

    void SetLayerRecursively(GameObject obj, int newLayer) {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    void DisableComponents () {
        for (int i = 0; i < ComponentsToDisable.Length; i++) {
            ComponentsToDisable[i].enabled = false;
        }
    }

    void AssignRemoteLayer() {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void OnDisable () {

        if (sceneCamera != null) {
            sceneCamera.gameObject.SetActive(true);
        }

        GameManager.UnRegisterPlayer(transform.name);
    }

}
