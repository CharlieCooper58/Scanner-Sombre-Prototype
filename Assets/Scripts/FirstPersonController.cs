using UnityEngine;
using System.Collections;
using UnityEngine.Animations;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using Animation;
using NetworkTools;
#endif

namespace PlayerController
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	//[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : NetworkBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Crouched walking speed of the character in m/s")]
		public float CrouchSpeed;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		// Serialized for debugging, un-serialize the crouched bool if you see this
		[SerializeField] bool crouched;
		[SerializeField] float crouchedHeight;
		[SerializeField] float baseHeight;
		[SerializeField] float crouchTime;
		bool crouchCoroutineIsRunning;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;


		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		Vector3 _horizontalVelocity;
		Vector3 currentVerticalVelocity;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif
		private CharacterController _controller;
		private PlayerInputHandler _input;
		private GameObject _mainCamera;
		AnimatorHandler animatorHandler;
		PlayerInputHandler inputHandler;
		WeaponBob equippedWeaponBob;

		private const float _threshold = 0.01f;


		// Netcode general
		NetworkTimer timer;
		const float k_serverTickRate = 240f; // 60 FPS
		const int k_bufferSize = 1024;

		// Netcode client specific
		CircularBuffer<StatePayload> clientStateBuffer;
		CircularBuffer<InputPayload> clientInputBuffer;
		StatePayload lastServerState;
		StatePayload lastProcessedState;

		// Netcode server specific
		CircularBuffer<StatePayload> serverStateBuffer;
		Queue<InputPayload> serverInputQueue;
		[SerializeField] float reconciliationThreshold = 3f;
		bool allowReconciliation = true;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return true; //_input.playerControls.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
			animatorHandler = GetComponent<AnimatorHandler>();
			inputHandler = GetComponent<PlayerInputHandler>();
			equippedWeaponBob = GetComponentInChildren<WeaponBob>();

			timer = new NetworkTimer(k_serverTickRate);
			clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
			clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

			serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
			serverInputQueue = new Queue<InputPayload>();
        }

		private void Start()
		{
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputHandler>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            //_playerInput = PlayerInputHandler
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
			if (IsLocalPlayer)
			{
				int localPlayerLayer = LayerMask.NameToLayer("Local Player");
				gameObject.layer = localPlayerLayer;
				GetComponentInChildren<Collider>().gameObject.layer = localPlayerLayer;
				CinemachineCameraTarget.layer = localPlayerLayer;
                CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
				if (vcam != null)
				{
					vcam.Follow = CinemachineCameraTarget.transform;
				}
            }

        }

        private void Update()
		{
			timer.Update(Time.deltaTime);
		}
		public void DebugTeleport()
		{
            _controller.enabled = false;
            transform.position = new Vector3(100, 100, 100);
            _controller.enabled = true;
			Debug.Log(transform.position);
        }
		private void FixedUpdate()
		{
			if (!(IsServer || IsLocalPlayer)) return;
			while (timer.ShouldTick())
			{
                if (IsClient)
				{
					HandleClientTick();
				}

				else if (IsServer)
					HandleServerTick();
			}
		}
		void HandleServerTick()
		{
			var bufferIndex = -1;
			while(serverInputQueue.Count > 0)
			{
				InputPayload inputPayload = serverInputQueue.Dequeue();
				bufferIndex = inputPayload.tick % k_bufferSize;

				StatePayload statePayload = ProcessMovement(inputPayload);
				serverStateBuffer.Add(statePayload, bufferIndex);
			}
			if (bufferIndex == -1) return;
			SendStateToClientRPC(serverStateBuffer.Get(bufferIndex));
		}
        StatePayload SimulateMovement(InputPayload inputPayload)
        {
			//Physics.simulationMode = SimulationMode.Script;
			//CalculateMovement(inputPayload.inputVector);
			//Physics.Simulate(timer.MinTimeBetweenTicks);
			//Physics.simulationMode = SimulationMode.FixedUpdate;
			return ProcessMovement(inputPayload);
        }

        [ClientRpc]
		void SendStateToClientRPC(StatePayload statePayload)
		{
			if (!IsOwner) return;
			lastServerState = statePayload;
		}
		void HandleClientTick()
		{
			if (!IsClient) return;
			var currentTick = timer.currentTick;
			var bufferIndex = currentTick % k_bufferSize;

			InputPayload inputPayload = new InputPayload()
			{
				tick = currentTick,
				inputVector = _input.move
			};
			clientInputBuffer.Add(inputPayload, bufferIndex);
			SendInputToServerRPC(inputPayload);

			StatePayload statePayload = ProcessMovement(inputPayload);
			//Debug.Log(statePayload.position);
			clientStateBuffer.Add(statePayload, bufferIndex);

			HandleServerReconciliation();
        }
		void HandleServerReconciliation()
		{
			if (!ShouldReconcile())
			{
				return;
			}
			float positionError;
			int bufferIndex;
			StatePayload rewindState = default;

			bufferIndex = lastServerState.tick % k_bufferSize;
			if (bufferIndex - 1 < 0) return; // Not enough information to reconcile
			rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;
			positionError = Vector3.Distance(clientStateBuffer.Get(bufferIndex).position, rewindState.position);
			if (positionError > reconciliationThreshold)
			{
                Debug.Log(rewindState.position);
                Debug.Log(positionError);
                ReconcileState(rewindState);
			}

			lastProcessedState = lastServerState;
		}
		bool ShouldReconcile()
		{
			bool isNewServerState = !lastServerState.Equals(default);
			bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) || !lastProcessedState.Equals(lastServerState);

			return isNewServerState && isLastStateUndefinedOrDifferent;// && allowReconciliation;
		}
		void ReconcileState(StatePayload rewindState)
		{
			_controller.enabled = false;
			transform.position = rewindState.position;
			transform.rotation = rewindState.rotation;

			_controller.enabled = true;
            Debug.Log(rewindState.position);
			Debug.Log(transform.position);
            // if (rewindState.Equals(lastServerState)) return;

            clientStateBuffer.Add(rewindState, rewindState.tick);

			// Replay all inputs from the rewind state to the current state
			int tickToReplay = lastServerState.tick;

			while(tickToReplay <= timer.currentTick)
			{
				int bufferIndex = tickToReplay % k_bufferSize;
				StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
				clientStateBuffer.Add(statePayload, bufferIndex);
				tickToReplay++;
			}

            Debug.Log("Discrepancy found, reconciled");
			//allowReconciliation = false;
		}
		[ServerRpc]
		void SendInputToServerRPC(InputPayload inputPayload)
		{
			serverInputQueue.Enqueue(inputPayload);
		}

		StatePayload ProcessMovement(InputPayload input)
		{
			CalculateMovement(input.inputVector);
			return new StatePayload()
			{
				tick = input.tick,
				position = transform.position,
				rotation = transform.rotation,
				controllerHeight = _controller.height
			};
		}
        private void CalculateMovement(Vector2 inputVector)
        {
            JumpAndGravity();
            GroundedCheck();
            CombineGroundedAndVerticalMovement(inputVector);
            equippedWeaponBob.SetInputValues(_input.look, new Vector3(_input.move.x, 0.0f, _input.move.y).normalized, _horizontalVelocity+currentVerticalVelocity, Grounded);
        }
        private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void CombineGroundedAndVerticalMovement(Vector2 inputVector)
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed :(_input.crouch? CrouchSpeed: MoveSpeed);

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (inputVector == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = _horizontalVelocity.magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? inputVector.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, timer.MinTimeBetweenTicks * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (inputVector != Vector2.zero)
			{
				// move
				inputDirection = transform.right * inputVector.x + transform.forward * inputVector.y;
			}
			_horizontalVelocity = inputDirection.normalized * _speed;

            _controller.Move(_horizontalVelocity * timer.MinTimeBetweenTicks + new Vector3(0.0f, _verticalVelocity, 0.0f) * timer.MinTimeBetweenTicks);
			
			
			//print(movementV3.magnitude);
            // move the player
            //_controller.Move(movementV3);

			//Physics.SyncTransforms();
		}

		public bool TryJump()
		{
			if (Grounded && _jumpTimeoutDelta <= 0.0f)
			{
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				return true;
            }
			return false;
        }
		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}
				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= timer.MinTimeBetweenTicks;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= timer.MinTimeBetweenTicks;
				}

				// if we are not grounded, do not jump
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * timer.MinTimeBetweenTicks;
			}
		}

		public bool TryCrouch()
		{
			// Try to crouch and return whether we were successful
			if (Grounded && !crouched && !crouchCoroutineIsRunning)
			{
                animatorHandler.PlayTargetAnimation("Crouch", 1);
				StartCoroutine("CrouchOrUncrouchCoroutine", true);
                return true;
			}
			return false;
		}
		public bool TryUncrouch()
		{
			if (crouched && !crouchCoroutineIsRunning)
			{
                StartCoroutine("CrouchOrUncrouchCoroutine", false);
                animatorHandler.PlayTargetAnimation("Uncrouch", 1);
				return true;
            }
			return false;
		}

		IEnumerator CrouchOrUncrouchCoroutine(bool goingDown)
		{
			if (crouchCoroutineIsRunning)
			{
				yield return null;
			}
			crouchCoroutineIsRunning = true;
            float crouchLerp = (baseHeight - crouchedHeight) / crouchTime;

            if (goingDown)
			{
				while(_controller.height > crouchedHeight)
				{
					_controller.height -= crouchLerp*Time.deltaTime;
					yield return null;
				}
				_controller.height = crouchedHeight;
				crouched = true;
			}
            else
            {
                while (_controller.height < baseHeight)
                {
                    _controller.height += crouchLerp*Time.deltaTime;
					Debug.Log(_controller.height);
					yield return null;
                }
                _controller.height = baseHeight;
                crouched = false;
            }
			crouchCoroutineIsRunning = false;
        }

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}