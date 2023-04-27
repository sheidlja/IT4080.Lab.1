using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SuperSmartDeluxeChatServer : ChatServer {
    public It4080.ChatUi chatUi;


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (chatUi != null) {
            chatUi.OnTextSubmitted += OnChatUiTextSubmitted;
        }

        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += HostOnClientConnected;
        } else {
            NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnected;
        }
    }


    protected override void ClientDisplayMessage(string message, string from) {
        if (chatUi == null) {
            base.ClientDisplayMessage(message, from);
        } else {
            string fromDisplay = from;
            if (from == NetworkManager.LocalClientId.ToString()) {
                fromDisplay = "you";
            } else if (from == "0") {
                fromDisplay = "host";
            }
            chatUi.AddMessage(fromDisplay, message);
        }
    }


    private void HostOnClientConnected(ulong clientId) {
        ServerSendSingleClientMessage("Hey, it is me the cool ChatServer.  How is it going?", clientId);
    }


    private void ClientOnClientDisconnected(ulong clientId) {
        chatUi.DisplayText("It's me again, the cool ChatServer...looks like you've lost your connection.");
    }


    // -------------------------------------------------------------------------
    // ChatUi interface.  All of this is running on the client.
    // -------------------------------------------------------------------------
    private void ParseAndSendDirectMessage(string message) {
        List<string> parts = new List<string>(message.Split(" "));
        string clientIdStr = parts[0].Replace("@", "");
        ulong toClientId = 999999;
        try {
            if (clientIdStr == "0") {
                toClientId = 0;
            } else {
                toClientId = ulong.Parse(clientIdStr);
            }
        } catch (Exception e) {
            chatUi.DisplayText($"invalid user \"{clientIdStr}\"");
            return;
        }

        parts.RemoveAt(0);
        string newMessage = string.Join(" ", parts);

        RequestSendDirectMessageServerRpc(newMessage, toClientId);
    }


    public void ParseAndRequestSendMessage(string message) {
        if (message.StartsWith("@")) {
            ParseAndSendDirectMessage(message);
        } else {
            RequestSendMessageServerRpc(message);
        }
    }


    private void OnChatUiTextSubmitted(string message) {
        if (NetworkManager.Singleton.IsClient) {
            ParseAndRequestSendMessage(message);
        } else {
            chatUi.DisplayText($"Not connected, message not sent:  {message}");
        }
    }

}