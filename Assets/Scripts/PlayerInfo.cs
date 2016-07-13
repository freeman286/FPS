using UnityEngine;

public class PlayerInfo : MonoBehaviour {

    public PlayerWeapon[] weapons;

    private bool connected = false;

    private string[] Selectedweapons;

    private PlayerWeapon primaryWeapon;

    private PlayerWeapon secondaryWeapon;

    private bool dualWield;

    void Start() {
        OnGUI();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(220, 15, 200, 500));
        GUILayout.BeginVertical();

        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        if (primaryWeapon == null) {
            for (int i = 0; i < weapons.Length; i++) {
                if (weapons[i].primary) {
                    primaryWeapon = weapons[i];
                    break;
                }
            }
        }
        else {
            GUILayout.Label("Primary: " + primaryWeapon.name);
        }
        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        for (int i = 0; i < weapons.Length; i++)
        {
            if ((weapons[i].primary && GUILayout.Button(weapons[i].name)))
            {
                primaryWeapon = weapons[i];
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(430, 15, 200, 500));
        GUILayout.BeginVertical();

        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        if (secondaryWeapon == null) {
            for (int i = 0; i < weapons.Length; i++) {
                if (!weapons[i].primary) {
                    secondaryWeapon = weapons[i];
                    break;
                }
            }
        } else {
            GUILayout.Label("Secondary: " + secondaryWeapon.name);
            if (secondaryWeapon.dualWieldable) {
                dualWield = GUILayout.Toggle(dualWield, "Dual Wield");
            } else {
                dualWield = false;
            }
        }
        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        for (int i = 0; i < weapons.Length; i++) {
            if (!weapons[i].primary && GUILayout.Button(weapons[i].name)) {
                secondaryWeapon = weapons[i];
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public void Connected()
    {
        connected = true;
    }

    public PlayerWeapon GetPrimaryWeapon() {
        return primaryWeapon;
    }

    public PlayerWeapon GetSecondaryWeapon() {
        return secondaryWeapon;
    }

    public bool DualWielding() {
        return dualWield;
    }
}
