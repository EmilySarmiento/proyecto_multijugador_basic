using Photon.Pun; // Import Photon library for networking
using Photon.Realtime; // Import library for player management in networking
using TMPro; // Import library for 3D text
using System.Collections; // Import library for collections
using System.Collections.Generic; // Import library for generic collections
using UnityEngine; // Import main Unity library
using UnityEngine.UI; // Import library for user interface
using Hashtable = ExitGames.Client.Photon.Hashtable; // Alias for Photon's Hashtable class

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable // Class that controls the player
{
    // User interface variables
    [SerializeField] private Image healthbarImage, healthbarImage2; // Health bars
    [SerializeField] private GameObject ui; // Player UI
    [SerializeField] private GameObject cameraHolder; // Camera holder

    // Player configuration variables
    [SerializeField] private float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform; // Transform to check if player is grounded

    [SerializeField] private Item[] items; // Array of items player can use

    private int itemIndex; // Current item index
    private int previousItemIndex = -1; // Previous item index

    private const float maxHealth = 100f; // Maximum player health
    private float currentHealth = maxHealth; // Current player health

    private Rigidbody rb; // Player Rigidbody component
    private PhotonView PV; // PhotonView component for networking
    private CharacterController characterController; // CharacterController component for movement

    private Vector3 moveInput; // Player movement input
    private float verticalLookRotation; // Vertical camera rotation
    private bool grounded; // Is player grounded?

    private PlayerManager playerManager; // Reference to PlayerManager

    void Awake()
        {
            // Initialize components
            rb = GetComponent<Rigidbody>();
            PV = GetComponent<PhotonView>();
            characterController = GetComponent<CharacterController>();

            // Check if CharacterController is present
            if (characterController == null)
            {
                Debug.LogError("CharacterController is not assigned.");
            }

            // Find associated PlayerManager
            playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
        }

    void Start()
        {
            // If this is the local player, equip first item
            if (PV.IsMine)
            {
                EquipItem(0);
            }
            else
            {
                // If not local player, destroy camera and UI
                Destroy(GetComponentInChildren<Camera>().gameObject);
                Destroy(rb);
                Destroy(ui);
            }
        }

     void Update()
        {
            // Only execute code if this is the local player
            if (!PV.IsMine) return;

            // Handle movement, looking, item switching, and item usage
            HandleMovement();
            HandleLook();
            HandleItemSwitching();
            HandleItemUsage();
            CheckFallDeath();
        }

        private void HandleMovement()
        {
            // Check if player is grounded
            if (characterController.isGrounded)
            {
            // Get movement input
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            moveInput = Vector3.ClampMagnitude(moveInput, 1f);
            moveInput = transform.TransformDirection(moveInput) * walkSpeed;

            // Handle jumping
            if (Input.GetKeyDown(KeyCode.Space))
                {
                    moveInput.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y); // Calculate jump force
                }
            }

            // Apply gravity
            moveInput.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(moveInput * Time.deltaTime); // Move player
        }

       private void HandleLook()
        {
            // Handle horizontal player rotation
            transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

            // Handle vertical camera rotation
            verticalLookRotation = Mathf.Clamp(verticalLookRotation + Input.GetAxisRaw("Mouse Y") * mouseSensitivity, -45f, 45f);
            cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
        }


        private void HandleItemSwitching()
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (Input.GetKeyDown((i + 1).ToString()))
                    {
                        EquipItem(i);
                        break;
                    }
                }

                if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
                {
                    EquipItem((itemIndex + 1) % items.Length);
                }
                else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
                {
                    EquipItem((itemIndex - 1 + items.Length) % items.Length);
                }
            }

        private void HandleItemUsage()
        {
            if (Input.GetMouseButtonDown(0))
            {
                items[itemIndex].Use();
            }
        }

        private void CheckFallDeath()
        {
            if (transform.position.y < -10f)
            {
                Die();
            }
        }

        private void EquipItem(int _index)
        {
            if (_index == previousItemIndex) return;

            itemIndex = _index;
            items[itemIndex].itemGameObject.SetActive(true);

            if (previousItemIndex != -1)
            {
                items[previousItemIndex].itemGameObject.SetActive(false);
            }

            previousItemIndex = itemIndex;

            if (PV.IsMine)
            {
                Hashtable hash = new Hashtable { { "itemIndex", itemIndex } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
            {
                EquipItem((int)changedProps["itemIndex"]);
            }
        }

        public void SetGroundedState(bool _grounded)
	    {
		    grounded = _grounded;
	    }


	    public void TakeDamage(float damage)
	    {
		    PV.RPC(nameof(RPC_TakeDamage), PV.Owner, damage);
	    }

	    [PunRPC]
	    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
	    {
		    currentHealth -= damage;

		    healthbarImage2.fillAmount = currentHealth / maxHealth;

		    healthbarImage.fillAmount = currentHealth / maxHealth;

		    if (currentHealth <= 0)
		    {
			    Die();
			    PlayerManager.Find(info.Sender).GetKill();
		    }
	    }

	    void Die()
	    {
		    playerManager.Die();
	    }
}