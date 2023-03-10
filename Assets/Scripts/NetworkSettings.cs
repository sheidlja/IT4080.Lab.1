using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using Unity.Netcode;

namespace It4080
{
    public class NetworkSettings : MonoBehaviour
    {
        public Button btnHost;
        public Button btnServer;
        public Button btnClient;
        public TMPro.TMP_InputField inIpAddress;
        public TMPro.TMP_InputField inPort;
        public TMPro.TMP_Text txtStatus;
        public GameObject ControlsContainer;

        public event Action<IPAddress, ushort> startServer;
        public event Action<IPAddress, ushort> startHost;
        public event Action<IPAddress, ushort> startClient;

        private IPAddress ipAddress = IPAddress.Parse("0.0.0.0");
        private ushort port = 0;


        void Start()
        {
            btnHost.onClick.AddListener(BtnHostOnClick);
            btnServer.onClick.AddListener(BtnServerOnClick);
            btnClient.onClick.AddListener(BtnClientOnClick);
        }

        private bool populateVars()
        {
            bool isValid = false;

            try {
                ipAddress = IPAddress.Parse(inIpAddress.text);
                port = ushort.Parse(inPort.text);
                isValid = true;
            } catch(Exception e) {
                Debug.LogError($"Ip/Port format:  {e.Message}");
            }

            return isValid;
        }


        // ----------------------
        // Events
        // ----------------------
        private void BtnHostOnClick()
        {
            if (startHost != null && populateVars())
            {
                startHost.Invoke(ipAddress, port);
            }            
        }


        private void BtnServerOnClick()
        {
            if (populateVars())
            {
                startServer.Invoke(ipAddress, port);
            }                
        }


        private void BtnClientOnClick()
        {
            if (populateVars())
            {
                startClient.Invoke(ipAddress, port);
            }                
        }

        // ----------------------
        // Public
        // ----------------------
        public void hide(bool should = true)
        {            
            ControlsContainer.SetActive(!should);
        }

        public void show(bool should = true)
        {
            this.hide(!should);
        }

        public void setStatusText(string msg)
        {
            txtStatus.text = msg;
        }
    }
}
