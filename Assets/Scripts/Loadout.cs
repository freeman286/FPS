using UnityEngine;

[System.Serializable]
public class Loadout : MonoBehaviour {

    public PlayerWeapon[] weapons;

    private bool connected = false;

    private string[] Selectedweapons;

    private PlayerWeapon primaryWeapon;

    private PlayerWeapon secondaryWeapon;

    void Update () {
        OnGUI();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 150, 200, 500));
        GUILayout.BeginVertical();

        if (primaryWeapon == null) {
            for (int i = 0; i < weapons.Length; i++) {
                if (weapons[i].primary) {
                    primaryWeapon = weapons[i];
                    break;
                }
            }
        } else {
            GUILayout.Label("Primary: " + primaryWeapon.name);
        }
        for (int i = 0; i < weapons.Length; i++) {
            if ((weapons[i].primary && GUILayout.Button(weapons[i].name))) {
                primaryWeapon = weapons[i];
            }
        }

        if (secondaryWeapon == null) {
            for (int i = 0; i < weapons.Length; i++) {
                if (!weapons[i].primary) {
                    secondaryWeapon = weapons[i];
                    break;
                }
            }
        } else {
            GUILayout.Label("Secondary: " + secondaryWeapon.name);
        }
        for (int i = 0; i < weapons.Length; i++) {
            if (!weapons[i].primary && GUILayout.Button(weapons[i].name)) {
                secondaryWeapon = weapons[i];
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public void Connected () {
        connected = true;
    }

    public PlayerWeapon GetPrimaryWeapon() {
        return primaryWeapon;
    }

    public PlayerWeapon GetSecondaryWeapon() {
        return secondaryWeapon;
    }
}
