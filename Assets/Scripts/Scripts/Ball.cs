using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


    public class Ball : NetworkBehaviour
    {

        void OnCollisionEnter(Collision collision)
        {
            if (IsServer)
            {
                Destroy(gameObject);
            }
        }
    }
