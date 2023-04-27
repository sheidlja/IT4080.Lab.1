using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace It4080 {
    public class PowerUpSpawner : NetworkBehaviour {
        public Rigidbody powerUp;

        public bool spawnOnLoad = true;
        public float spawnDelay = 3.0f;


        // The current power up.  When this is destroyed, this will
        // be null.  This is the network spawned instance of powerUp.
        private Rigidbody curPowerUp = null;
        // keeps track of time left after a power up has been
        // picked up.
        private float timeUntilSpawn = 0.0f;


        public void Start() {
        }


        public void Update() {
            if (IsServer)
            {
                ServerUpdate();
            }
        }


        private void ServerUpdate()
        {
            if (timeUntilSpawn > 0f) {
                timeUntilSpawn -= Time.deltaTime;
                if (timeUntilSpawn <= 0) {
                    SpawnPowerUp();
                }
            } else if(curPowerUp == null){
                timeUntilSpawn = spawnDelay;
            }
        }


        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            if (IsServer) {
                HostOnNetworkSpawn();
            }
        }


        private void HostOnNetworkSpawn() {
            if (powerUp != null && spawnOnLoad) {
                SpawnPowerUp();
            }
        }


        private void SpawnPowerUp() {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y = 1;

            Rigidbody instantiatedPowerUP =
                Instantiate(powerUp, spawnPosition, Quaternion.identity);

            instantiatedPowerUP.GetComponent<NetworkObject>().Spawn();

            curPowerUp = instantiatedPowerUP;
        }
    }
}