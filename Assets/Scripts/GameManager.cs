using UnityEngine;
using System.Collections.Generic;

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

    public static void RegisterPlayer(string _netID, Player _player) {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
        kills.Add(_playerID, 0);
        deaths.Add(_playerID, 0);
    }

    public static void UnRegisterPlayer(string _playerID) {
        players.Remove(_playerID);
        kills.Remove(_playerID);
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
        GUILayout.BeginArea(new Rect(10, 120, 200, 500));
        GUILayout.BeginVertical();

        GUILayout.Label("My IP: " + Network.player.ipAddress);

        GUILayout.Label("Frames per second:");
        GUILayout.Label((1.0f / Time.deltaTime).ToString());

        GUILayout.Label("Players in game:");
        foreach (string _playerID in players.Keys) {

            string _text = players[_playerID].transform.name + " Kills:" + kills[_playerID] + " Deaths:" + deaths[_playerID];

            if (players[_playerID].isDead) {
                GUILayout.Label("<color=grey>" + _text + "</color>");
            }
            else if (players[_playerID].isDamaged) {
                GUILayout.Label("<color=red>" + _text + "</color>");
            }
            else {
                GUILayout.Label(_text);

            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    #endregion

}
