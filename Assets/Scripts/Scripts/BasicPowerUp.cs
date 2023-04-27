using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace It4080 {

    public class BasicPowerUp : NetworkBehaviour {

        void OnCollisionEnter(Collision collision) {
            if (IsServer) {
                // Since this powerup is spawned on the server (through the BulletSpawner),
                // we can use Destroy here instead of Despawn.  Spawned objects that
                // are destroyed by the server/host (server/host is the owner in this
                // case) are destoryed on the clients.
                Destroy(gameObject);
            }
        }
    }
}