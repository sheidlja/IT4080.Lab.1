using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace It4080
{
    public class BulletSpawner : NetworkBehaviour
    {
        public Rigidbody bulletPrefab;
        private float bulletSpeed = 40f;
        private float timeToLive = 3f;

        
        [ServerRpc]
            public void FireServerRpc(Color color, ServerRpcParams rpcParams = default)
        {
            Rigidbody newBullet = Instantiate(
                bulletPrefab,
                transform.position,
                transform.rotation);

            newBullet.GetComponent<NetworkObject>().SpawnWithOwnership(
                rpcParams.Receive.SenderClientId);
            newBullet.velocity = transform.forward * bulletSpeed;
            Destroy(newBullet.gameObject, timeToLive);
        }
    }
}
