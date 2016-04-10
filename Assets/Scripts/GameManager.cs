using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour {

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

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();
    private static Dictionary<string, int> kills = new Dictionary<string, int>();
    private static Dictionary<string, int> deaths = new Dictionary<string, int>();
    private static Dictionary<string, string> names = new Dictionary<string, string>();

    public static void RegisterPlayer(string _netID, Player _player) {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
        kills.Add(_playerID, 0);
        deaths.Add(_playerID, 0);
        names.Add(_playerID, CreateNewName(_playerID));
    }

    public static void UnRegisterPlayer(string _playerID) {
        players.Remove(_playerID);
        kills.Remove(_playerID);
        names.Remove(_playerID);
    }

    public static Player GetPlayer(string _playerID) {
        return players[_playerID];
    }

    public static void IncrPlayerScore(string _playerID) {
        kills[_playerID] += 1;
    }

    public static void IncrPlayerDeaths(string _playerID)  {
        deaths[_playerID] += 1;
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
            while (r < 0.9f && g < 0.9f && b < 0.9f)
            {
                r = Random.Range(0.1f, 1.0f);
                b = Random.Range(0.1f, 1.0f);
                g = Random.Range(0.1f, 1.0f);
            }
            Color _color = new Color(r, g, b);

            string _text = names[_playerID] + " Kills:" + kills[_playerID] + " Deaths:" + deaths[_playerID];

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

        string[] nameSyllables = { "MLG", "leet", "1337", "h4x", "face", "d00d", "kewl", "m8", "m80", "m88", "m9", "pwn", "pwnage", "FTW", " O RLY?", "4TW", "pew", "PewPew", "n00b", "nub", "livid", "friend", "dew", "coffee", "sniper", "sweg", "xtrem3", "skrub", "skrub lord" };
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
        

        if (Random.Range(0.0f, 1.0f) > 0.5f) {
            name = prefixes[Random.Range(0, prefixes.Length)].ToUpper() + " " + name;
        }

        return name;
    }

    string ColorToHex(Color32 color) {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
}
