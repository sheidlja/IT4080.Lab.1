using Unity.Netcode;
using UnityEngine;



namespace It4080
{

    public class Player : NetworkBehaviour
    {
        private float movementSpeed = 10f;
        private float rotationSpeed = 300f;
        private Camera _camera;

        private float fireRate = 1f;
        private float nextFire = 0f;

        private int serverColorIndex = 0;
        private static Color[] availColors = new Color[] {
            Color.black, Color.blue, Color.cyan,
            Color.gray, Color.green, Color.yellow,
            Color.red, Color.white, Color.clear
        };

        public NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();
        public NetworkVariable<int> netScore = new NetworkVariable<int>(5);
        public BulletSpawner bulletSpawner;
        public NetworkVariable<int> netTeamID = new NetworkVariable<int>(-1);
        public NetworkVariable<bool> canMovePastWall = new NetworkVariable<bool> (false);
        public NetworkVariable<bool> ballsGrabbed = new NetworkVariable<bool>(false);
        public TMPro.TMP_Text txtScoreDisplay;


        public override void OnNetworkSpawn()
        {
            _camera = transform.Find("Camera").GetComponent<Camera>();
            _camera.enabled = IsOwner;

            //netPlayerColor.OnValueChanged += OnPlayerColorChanged;
            //netPlayerColor.Value = availColors[serverColorIndex];
            netScore.OnValueChanged += OnScoreChanged;

            DisplayLives();
            UpdateScoreDisplay();
            ApplyPlayerColor();
        }

        void Update()
        {
            if (IsOwner)
            {
                OwnerUpdate();
            }
        }
        // -------------------
        // Private
        // -------------------
        private void ApplyPlayerColor()
        {
            if (netTeamID.Value == 0)
            {
                GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            
        }


        private void UpdateScoreDisplay()
        {
            if (IsOwner)
            {
                Debug.Log($"[{NetworkManager.Singleton.LocalClientId}] My Score = {netScore.Value}");
            }
        }


        // horiz changes y rotation or x movement if shift down, vertical moves forward and back.
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

            if (!isShiftKeyDown)
            {
                y_rot = Input.GetAxis("Horizontal");
            }

            Vector3 rotVect = new Vector3(0, y_rot, 0);
            rotVect *= rotationSpeed * delta;
            return rotVect;
        }


        // handles movement from input and handling of "fire1" pressed.
        private void OwnerUpdate()
        {
            Vector3 moveBy = CalcMovementFromInput(Time.deltaTime);
            Vector3 rotateBy = CalcRotationFromInput(Time.deltaTime);
            RequestPositionForMovementServerRpc(moveBy, rotateBy);

            if (ballsGrabbed.Value == true)
            {
                if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate;
                    bulletSpawner.FireServerRpc(netPlayerColor.Value);
                }
            }
        }


        private void ServerNextColor()
        {
            serverColorIndex += 1;
            if (serverColorIndex > availColors.Length - 1)
            {
                serverColorIndex = 0;
            }
            netPlayerColor.Value = availColors[serverColorIndex];
        }


        private void ServerHandleBulletCollision(GameObject bullet)
        {
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            netScore.Value -= 1;
            // Cannot call RPC because we might not be the owner and the RPC
            // requires ownership.  We are on the host, so we can just do the
            // color change instead of requesting it.
            ServerNextColor();

            ulong owner = bullet.GetComponent<NetworkObject>().OwnerClientId;
            Player otherPlayer =
                NetworkManager.Singleton.ConnectedClients[owner].PlayerObject.GetComponent<Player>();
            //otherPlayer.netScore.Value += 1;

            Destroy(bullet);
        }


        // -------------------
        // Events
        // -------------------
        private void OnPlayerColorChanged(Color previous, Color current)
        {
            ApplyPlayerColor();
        }


        private void OnScoreChanged(int previous, int current)
        {
            UpdateScoreDisplay();
            txtScoreDisplay.text = current.ToString();
        }


        void OnCollisionEnter(Collision collision)
        {
            if (IsServer)
            {
                if (collision.gameObject.CompareTag("Bullet"))
                {
                    ServerHandleBulletCollision(collision.gameObject);
                }

                if (collision.gameObject.CompareTag("PowerUp"))
                {
                    canMovePastWall.Value = true;
                }

                if (collision.gameObject.CompareTag("Ball"))
                {
                    ballsGrabbed.Value = true;
                }

            }
        }

        public void PlayerDeath(GameObject Player)
        {
            if (netScore.Value < 1)
            {
                Destroy(Player.gameObject);
            }
        }


        // -------------------
        // Public
        // -------------------
        [ServerRpc]
        public void RequestNextColorServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ServerNextColor();
            //Debug.Log($"host color index = {hostColorIndex} for {serverRpcParams.Receive.SenderClientId}");
        }

        [ServerRpc]
        public void RequestPositionForMovementServerRpc(
                Vector3 posChange, Vector3 rotChange,
                ServerRpcParams serverRpcParams = default)
        {


            Vector3 allowedPosChange = posChange;

            if (canMovePastWall.Value == false)
            {
                if (netTeamID.Value == 0)
                {
                    if (transform.position.x + allowedPosChange.x > 0f)
                    {
                        allowedPosChange.x = 0;
                    }
                }
                if (netTeamID.Value == 1)
                {
                    if (transform.position.x + allowedPosChange.x < 0f)
                    {
                        allowedPosChange.x = 0;
                    }
                }
                transform.Translate(allowedPosChange);
                transform.Rotate(rotChange);
            }

        }

        public void DisplayLives()
        {
            txtScoreDisplay.text = netScore.Value.ToString();
        }
    }
}