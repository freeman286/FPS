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

    public static void RegisterPlayer(string _netID, Player _player) {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
    }

    public static void UnRegisterPlayer(string _playerID) {
        players.Remove(_playerID);
    }

    public static Player GetPlayer(string _playerID) {
        return players[_playerID];
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

        GUILayout.Label("<size=20>Players in game:</size>");
        foreach (string _playerID in players.Keys) {

            string _name = players[_playerID].transform.name;

            if (players[_playerID].isDead)
            {
                GUILayout.Label("<color=grey><size=20>" + _name + "</size></color>");
            }
            else if (players[_playerID].isDamaged) {
                GUILayout.Label("<color=red><size=20>" + _name + "</size></color>");
            }
            else {
                GUILayout.Label("<size=20>" + _name + "</size>");

            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    #endregion

}
