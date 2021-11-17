using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Assets.Scripts.PlayerInput
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        private float speed;

        private float playerSpeed;

        [SerializeField]
        private float jumpForce;

        private PlayerControls playerControls;
        private Rigidbody2D rigidBody;

        private bool isGrounded = false;
        private bool boostMovement = false;

        private void Awake()
        {
            playerControls = new PlayerControls();
            rigidBody = GetComponent<Rigidbody2D>();
            playerSpeed = speed;
        }

        private void OnEnable()
        {
            // subscribe to events only on performed, so the event doesn't get triggered thrice
            playerControls.Gameplay.Jump.performed += OnJump;
            playerControls.Gameplay.BoostSpeed.performed += OnBoostMovement;

            // enable controls
            playerControls.Gameplay.Enable();
        }

        private void Update()
        {
            Move();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(Statics.GROUND_TAG)
                // so player doesn't get stuck at specific obstacles
                || other.gameObject.CompareTag(Statics.OBSTACLE_TAG))
            {
                isGrounded = true;
            }
        }

        private void OnDisable()
        {
            // disable controls
            playerControls.Gameplay.Disable();
        }

        #region Player Input Methods
        private void OnJump(InputAction.CallbackContext context)
        {
            // if player currently on ground he/she is able to jump
            if (isGrounded)
            {
                isGrounded = false;
                rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// Method which is responsible for moving the character and 
        /// assigning the correct and flipping the sprite based on the direction the player is facing
        /// </summary>
        private void Move()
        {
            // if player presses specific button, he moves faster
            playerSpeed = boostMovement ? speed * 1.5f : speed;

            // read input of the player
            Vector2 input = playerControls.Gameplay.Movement.ReadValue<Vector2>();

            // normalize will scale down the values until the biggest is a value of 1 (has a length of 1)
            // limit movement to only x axis
            Vector2 movement = new Vector2(input.x, 0).normalized * (playerSpeed * Time.deltaTime);
            rigidBody.transform.Translate(movement, Space.World);

            // flip player
            FlipPlayer(input.x);
        }

        private void OnBoostMovement(InputAction.CallbackContext context)
        {
            // if context is triggered and value is greater than zero, the player moves faster
            boostMovement = (context.action.triggered && context.action.ReadValue<float>() > 0);
        }

        /// <summary>
        /// Method for changing the direction the player is facing or wants to walk to
        /// Instead of an if - else statement, there are two if statements used to catch up the direction = 0 case
        /// If 0 is reached the direction or the facing of the player should stay instead of change
        /// </summary>
        private void FlipPlayer(float xDirection)
        {
            bool facingRight = false;
            if (xDirection < 0)
            {
                facingRight = !facingRight;
                Flip(facingRight);
            }
            if (xDirection > 0)
            {
                Flip(facingRight);
            }

        }

        private void Flip(bool facingRight)
        {
            Vector2 scale = rigidBody.transform.localScale;
            if (((facingRight) && (scale.x < 0)) || ((!facingRight) && (scale.x > 0)))
            {
                scale.x *= -1;
            }
            rigidBody.transform.localScale = scale;
        }
        #endregion
    }
}