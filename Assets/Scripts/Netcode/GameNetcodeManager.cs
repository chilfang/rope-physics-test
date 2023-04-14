using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;

public class GameNetcodeManager : NetworkBehaviour {
    [SerializeField] 
    private GameObject playerServerPrefab;
    [SerializeField]
    private GameObject playerClientPrefab;

    static string Ip = "";
    static string Port = "";
    void OnGUI() {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 300));
            GUILayout.Label("IP");
            GUILayout.TextField(Ip);
            GUILayout.Label("Port");
            GUILayout.TextField(Port);

            GUILayout.EndArea();
        }
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            StartButtons();
        } else {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    static void StartButtons() {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = Ip;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = System.Convert.ToInt32(Port);
    }

    static void StatusLabels() {
        var mode = NetworkManager.Singleton.IsHost ? "Host" :
            NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    
    static void SubmitNewPosition() {
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change")) {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient) {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<GameNetcodePlayer>().Move();
            } else {
                try {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<GameNetcodePlayer>();
                    player.Move();
                } catch (System.Exception e) {
                    Debug.Log(e);
                }
            }
        }
    }
    

    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn
    public void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default) {
        GameObject newPlayer = Instantiate(rpcParams.Receive.SenderClientId == OwnerClientId ? playerServerPrefab : playerClientPrefab);
        var netObj = newPlayer.GetComponent<NetworkObject>();
        //Debugger.Singleton.objectsHolder.Add(newPlayer);
        netObj.SpawnAsPlayerObject(rpcParams.Receive.SenderClientId, true);
    }

    public override void OnNetworkSpawn() {
        SpawnPlayerServerRpc();
    }
}
