using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace It4080
{

    public class Player : NetworkBehaviour
    {

        private float movementSpeed = 100f;
        private float rotationSpeed = 300f;
        public BulletSpawner _bulletSpawner;
        public Bullet BulletScript;
        public NetworkVariable<int> Score = new NetworkVariable<int>(5);

        public TMPro.TMP_Text txtScoreDisplay;

        
        private static Color[] availColors = new Color[]
        {
            Color.black, Color.blue, Color.cyan,
            Color.gray, Color.green, Color.yellow
        };

        private int hostColorIndex = 0;

        public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();


        public override void OnNetworkSpawn()
        {
            Camera camera;
            camera = transform.Find("Camera").GetComponent<Camera>();
            camera.enabled = IsOwner;

            Score.OnValueChanged += ClientOnScoreChanged;
            DisplayScore();

            txtScoreDisplay.text = $"P: {NetworkManager.Singleton.LocalClientId}";

            _bulletSpawner = transform.Find("Cylinder").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();
            if (IsHost)
            {
                _bulletSpawner.BulletDamage.Value = 1;
            }
            netPlayerColor.OnValueChanged += OnPlayerColorChanged;
        }

        private void ClientOnScoreChanged (int previous, int current)
        {
            DisplayScore();
        }


        public void ApplyPlayerColor()
        {
            GetComponent<Renderer>().material.color = netPlayerColor.Value;
        }


        public void OnPlayerColorChanged(Color previous, Color current)
        {
            ApplyPlayerColor();
        }


        public void Update()
        {
            if (IsOwner)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    //RequestNextColorServerRpc();
                    _bulletSpawner.FireServerRPC();
                }
            }
        }

        public void DisplayScore()
        {
            txtScoreDisplay.text = Score.Value.ToString();
        }

        [ServerRpc]
        void RequestNextColorServerRpc(ServerRpcParams serverRpcParams = default)
        {
            hostColorIndex += 1;
            if (hostColorIndex > availColors.Length - 1)
            {
                hostColorIndex = 0;
            }

            Debug.Log($"host color index = {hostColorIndex} for {serverRpcParams.Receive.SenderClientId}");
            netPlayerColor.Value = availColors[hostColorIndex];

        }

        private void HostHandleBulletCollision(GameObject bullet)
        {
            Bullet BulletScript = bullet.GetComponent<Bullet>();
            Destroy(bullet);
            ulong ownerClientId = bullet.GetComponent<NetworkObject>().OwnerClientId;
            Player otherPlayer = NetworkManager.Singleton.ConnectedClients[ownerClientId].PlayerObject.GetComponent<Player>();
           //otherPlayer.Score.Value += BulletScript.Damage.Value;
        }

        private void HostHandlePowerUpPickup(Collider other)
        {
            if (!_bulletSpawner.IsAtMaxDamage())
            {
                _bulletSpawner.IncreaseDamage();
                other.GetComponent<NetworkObject>().Despawn();
            }
        }

        [ServerRpc]
        
        public void RequestSetScoreServerRPC(int value)
        {
            Score.Value = value;
        }

        [ServerRpc]
        public void RequestPositionForMovementServerRPC(Vector3 posChange, Vector3 rotChange, ServerRpcParams serverRpcParams = default)
        {
            transform.Translate(posChange);
            transform.Rotate(rotChange);
        }

        public void OnCollisionEnter(Collision collision)
        {
           if (IsHost)
            {
                if (collision.gameObject.CompareTag("Bullet"))
                {
                    Score.Value -= 1;
                    HostHandleBulletCollision(collision.gameObject);
                    Destroy(collision.gameObject);
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
           if (IsHost)
            {
                if (other.gameObject.CompareTag("PowerUp"))
                {
                    HostHandlePowerUpPickup(other);
                }
            }
        }

        private Vector3 CalcMovementFromInput(float delta)
        {
            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float x_move = 0.0f;
            float z_move = Input.GetAxis("Vertical");

            if (isShiftKeyDown)
            {
                x_move = Input.GetAxis("Horizontal");
            }

            Vector3 moveVect = new Vector3(x_move, 0, z_move);
            moveVect *= movementSpeed * delta;
            return moveVect;
        }

        private Vector3 CalcRotationFromInput(float delta)
        {
            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float y_rot = 0.0f;

            if (isShiftKeyDown)
            {
                y_rot = Input.GetAxis("Horizontal");
            }

            Vector3 rotVect = new Vector3(0, y_rot, 0);
            rotVect *= rotationSpeed * delta;
            return rotVect;
        }
    }
}
