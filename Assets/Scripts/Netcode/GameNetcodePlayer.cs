using Unity.Netcode;
using UnityEngine;

public class GameNetcodePlayer : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        name = "Avatar" + OwnerClientId;

        if (IsOwner) {
            transform.Find("CameraPivot").gameObject.SetActive(true);
            Move();
        } else {
            transform.Find("Controller").gameObject.SetActive(false);

        }
    }

    public void Move() {
        if (NetworkManager.Singleton.IsServer) {
            transform.position = GetRandomPositionOnPlane();
        } else {
            var pos = GetRandomPositionOnPlane();
            SubmitPositionRequestServerRpc(pos);
            transform.position = pos;
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 position, ServerRpcParams rpcParams = default) {
        transform.position = position;
    }

    static Vector3 GetRandomPositionOnPlane() {
        return new Vector3(Random.Range(-10f, 10f), 3f, Random.Range(-10f, 10f));
    }

    void Update() {
        //transform.position = Position.Value;
    }
}
