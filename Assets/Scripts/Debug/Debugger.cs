using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//https://forum.unity.com/threads/player-log-file-location-changed-is-there-an-option-in-settings-somewhere-to-change-it-back.500955/#post-6257543
public class Debugger : NetworkBehaviour {
    private static Debugger _singleton;
    public static Debugger Singleton { get { return _singleton; } }
    private void Awake() {
        if (_singleton != null && _singleton != this) {
            Destroy(this.gameObject);
        } else {
            _singleton = this;
        }
    }


    public GameObject objectHolder;
    public List<GameObject> objectsHolder = new List<GameObject>();

    
    string filename = "";
    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }

    public void Log(string logString, string stackTrace, LogType type) {
        if (filename == "") {
            string d = System.Environment.GetFolderPath(
              System.Environment.SpecialFolder.Desktop) + "/unity_logs";
            System.IO.Directory.CreateDirectory(d);
            //filename = d + "/debugLog.txt";
            filename = d + "/debugLogClient" + NetworkManager.LocalClientId + ".txt";
            System.IO.File.AppendAllText(filename, "[START " + System.DateTime.Now.ToString() + "]\n");
        } else {
        }

        try {
            System.IO.File.AppendAllText(filename, logString + "\n");
        }
        catch { }
    }
}
