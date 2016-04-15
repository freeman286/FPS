using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class GameManager : NetworkBehaviour {

    public static GameManager instance;

    public MatchSettings matchSettings;

    void Awake () {
        if (instance != null) {
            Debug.Log("More than one GameManager in scene");
        }
        else {
            instance = this;
        }
    }

    #region Player tracking

    private const string PLAYER_ID_PREFIX = "Player ";

    public static Dictionary<string, Player> players = new Dictionary<string, Player>();
    public static Dictionary<string, string> names = new Dictionary<string, string>();

    public static void RegisterPlayer(string _netID, Player _player) {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
        names.Add(_playerID, CreateNewName(_playerID));
    }

    public static void UnRegisterPlayer(string _playerID) {
        players.Remove(_playerID);
        names.Remove(_playerID);
    }

    public static string[] Players() {
        return players.Keys.ToArray();
    }

    public static Player GetPlayer(string _playerID) {
        return players[_playerID];
    }

    public static int PlayerCount() {
        return players.Keys.Count;
    }

    void OnGUI () {
        GUILayout.BeginArea(new Rect(10, 120, 500, 700));
        GUILayout.BeginVertical();

        GUILayout.Label("My IP: " + Network.player.ipAddress);

        GUILayout.Label("Frames per second:");
        GUILayout.Label((1.0f / Time.deltaTime).ToString());

        GUILayout.Label("Players in game:");
        foreach (string _playerID in players.Keys) {

            float r = 0;
            float g = 0;
            float b = 0;

            Random.seed = int.Parse(Regex.Replace(_playerID, "[^0-9]", "")) * System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
            while (!((r < 0.3f || g < 0.3f || b < 0.3f) && (r > 0.7f || g > 0.7f || b > 0.7f))) {
                r = Random.Range(0.1f, 1.0f);
                b = Random.Range(0.1f, 1.0f);
                g = Random.Range(0.1f, 1.0f);
            }
            Color _color = new Color(r, g, b);

            string _text = names[_playerID] + " Kills:" + players[_playerID].kills + " Deaths:" + players[_playerID].deaths;

            if (players[_playerID].isDead) {
                GUILayout.Label("<color=grey>" + _text + "</color>");
            }
            else if (players[_playerID].isDamaged) {    
                GUILayout.Label("<color=red>" + _text + "</color>");
            }
            else {
                GUILayout.Label("<color=#" + ColorToHex(_color) + ">" + _text + "</color>");

            }   
        }


        GUILayout.EndVertical();    
        GUILayout.EndArea();
    }

    #endregion

    static string CreateNewName(string _playerID) {

        string[] nameSyllables = { "MLG", "leet", "1337", "h4x", "face", "d00d", "kewl", "m8", "m80", "m88", "m9", "pwn", "pwnage", "FTW", "O RLY?", "4TW", "pew", "PewPew", "n00b", "nub", "livid", "friend", "dew", "coffee", "sniper", "sweg", "xtrem3", "skrub", "skrub lord", "3million", "9000", "over 9000", "420", "360", "r3b3l" };
        string[] prefixes = { "Teh", "Mc", "Mr", "Teh Real", "Meh", "Lord", "Mastah", "th3", "It's" };

        Random.seed = int.Parse(Regex.Replace(_playerID, "[^0-9]", "")) * System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;

        string name = "";
        int numberOfSyllablesInFirstName = Random.Range(2, 4);
        for (int i = 0; i < numberOfSyllablesInFirstName; i++) {
            name += nameSyllables[Random.Range(0, nameSyllables.Length)].ToUpper();
            if (i != numberOfSyllablesInFirstName - 1)
            {
                name += " ";
            }
        }

        for (int i = 0; i < Random.Range(0, 2); i++) {
            name = prefixes[Random.Range(0, prefixes.Length)].ToUpper() + " " + name;
        }

        return name;
    }

    string ColorToHex(Color32 color) {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
}
