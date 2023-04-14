using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CubeFieldGenerator : NetworkBehaviour {
    //private NetworkVariable<GameObject> cubes = new NetworkVariable<GameObject>();
    //private NetworkList<GameObject> cubes 
    private List<GameObject> cubes = new List<GameObject>();

    const int range = 150;
    void Start() {
        //cubes.Initialize(this);

        /*
            for (int i = 0; i < 400; i++) {
                cubes.Value.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
                cubes.Value[^1].transform.localScale = new Vector3(Random.Range(3, 7), Random.Range(3, 7), Random.Range(3, 7));
                cubes.Value[^1].transform.parent = gameObject.transform;
                cubes.Value[^1].transform.localPosition = new Vector3(Random.Range(-range / 2, range / 2), Random.Range(0, range), Random.Range(-range / 2, range / 2));
                cubes.Value[^1].name = "Cube" + i;
            }
        */
    }

    [ServerRpc(RequireOwnership = false)]
    void GetCubeFieldServerRpc(ServerRpcParams rpcParams = default) {
        cubesFieldInfo[] cubes = new cubesFieldInfo[this.cubes.Count];

        for (int i = 0; i < this.cubes.Count; i++) {
            cubes[i].localPosition = this.cubes[i].transform.localPosition;
            cubes[i].localScale = this.cubes[i].transform.localScale;
            cubes[i].name = this.cubes[i].name;
        }

        SendCubeFieldClientRpc(cubes, new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
            }
        });
    }

    [ClientRpc]
    void SendCubeFieldClientRpc(cubesFieldInfo[] serverCubes, ClientRpcParams rpcParams = default) {
        if (IsOwner) return;

        foreach (var cube in serverCubes) {
            cubes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            cubes[^1].transform.localScale = cube.localScale;
            cubes[^1].transform.parent = gameObject.transform;
            cubes[^1].transform.localPosition = cube.localPosition;
            cubes[^1].name = cube.name;
        }
    }

    public override void OnNetworkSpawn() {
        if (NetworkManager.Singleton.IsServer) {
            for (int i = 0; i < 400; i++) {
                cubes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
                cubes[^1].transform.localScale = new Vector3(Random.Range(3, 7), Random.Range(3, 7), Random.Range(3, 7));
                cubes[^1].transform.parent = gameObject.transform;
                cubes[^1].transform.localPosition = new Vector3(Random.Range(-range / 2, range / 2), Random.Range(0, range), Random.Range(-range / 2, range / 2));
                cubes[^1].name = "Cube" + i;
            }

        } else {
            GetCubeFieldServerRpc();
        }
        
    }

}

public struct cubesFieldInfo : INetworkSerializable {
    public cubesFieldInfo(GameObject cube) {
        localScale = cube.transform.localScale;
        localPosition = cube.transform.localPosition;
        name = cube.name;
    }

    public Vector3 localScale;
    public Vector3 localPosition;
    public string name;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref localScale);
        serializer.SerializeValue(ref localPosition);
        serializer.SerializeValue(ref name);
    }
}
