using UnityEngine;

public class PlayerInfo : MonoBehaviour {

    public PlayerWeapon[] weapons;

    private bool connected = false;

    private string[] Selectedweapons;

    private PlayerWeapon primaryWeapon;

    private PlayerWeapon secondaryWeapon;

    void Start() {
        OnGUI();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 150, 200, 500));
        GUILayout.BeginVertical();

        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        if (primaryWeapon == null)
        {
            for (int i = Random.Range(1, 4); i < weapons.Length; i += Random.Range(2, 4))
            {
                if (weapons[i].primary)
                {
                    primaryWeapon = weapons[i];
                    break;
                }
            }
        }
        else {
            GUILayout.Label("Primary: " + primaryWeapon.name);
        }
        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        for (int i = Random.Range(1, 4); i < weapons.Length; i += Random.Range(2, 4))
        {
            if ((weapons[i].primary && GUILayout.Button(weapons[i].name)))
            {
                primaryWeapon = weapons[i];
            }
        }

        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        if (secondaryWeapon == null)
        {
            for (int i = Random.Range(1, 6); i < weapons.Length; i += Random.Range(2, 6))
            {
                if (!weapons[i].primary)
                {
                    secondaryWeapon = weapons[i];
                    break;
                }
            }
        }
        else {
            GUILayout.Label("Secondary: " + secondaryWeapon.name);
        }
        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        for (int i = Random.Range(1, 6); i < weapons.Length; i += Random.Range(2, 6))
        {
            if (!weapons[i].primary && GUILayout.Button(weapons[i].name))
            {
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
}
