using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour {
    private ulong[] singleClientId = new ulong[1];

    virtual protected void ClientDisplayMessage(string message, string from) {
        Debug.Log($"[{from}]: {message}");
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestSendMessageServerRpc(string message, ServerRpcParams serverRpcParams = default) {
        SendMessageClientRpc(message, serverRpcParams.Receive.SenderClientId.ToString());
    }


    [ClientRpc]
    public void SendMessageClientRpc(string message, string from, ClientRpcParams clientRpcParams = default) {
        ClientDisplayMessage(message, from);
    }

    // ---------------------
    // RPCs
    // ---------------------


    [ServerRpc(RequireOwnership = false)]
    public void RequestSendDirectMessageServerRpc(string message, ulong to, ServerRpcParams serverRpcParams = default) {
        ulong from = serverRpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(to)) {
            ServerReplyAsError($"Client {to} could not be found.  Message not sent '{message}'", from);
        } else {
            ServerSendSingleClientMessage($"<whisper> {message}", to, from);
            ServerSendSingleClientMessage($"<you whispered {to}>{message}", from, from);
        }
    }


    // -------------------------------------------------------------------------
    // SendMessageClientRpc convenience wrappers
    // -------------------------------------------------------------------------
    public void ServerSendSingleClientMessage(string message, ulong to, ulong from = ulong.MaxValue) {
        singleClientId[0] = to;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = singleClientId;

        string fromStr = from.ToString();
        if (from == ulong.MaxValue) {
            fromStr = "SERVER";
        }
        SendMessageClientRpc(message, fromStr, rpcParams);
    }


    public void ServerReplyAsError(string message, ulong to) {
        ServerSendSingleClientMessage($"Error:  {message}", to);
    }

}