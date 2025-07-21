using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

namespace Oculus.Interaction.Locomotion
{
    public class VRFlyCamera : MonoBehaviour, ILocomotionEventHandler, IDeltaTimeConsumer
    {
        [Header("Character")]
        [SerializeField]
        private CharacterController _characterController;

        [Header("VR Player")]
        /// <summary>
        /// Root of the actual VR player so it
        /// can be sync with with capsule.
        /// If you provided a _playerEyes you must also
        /// provide a _playerOrigin.
        /// </summary>
        [SerializeField]
        [Tooltip("Root of the actual VR player so it can be sync with with capsule. If you provided a _playerEyes you must also provide a _playerOrigin.")]
        private Transform _playerOrigin;

        /// <summary>
        /// Eyes of the actual VR player so it
        /// can be sync with the capsule.
        /// If you provided a _playerOrigin you must also
        /// provide a _playerEyes.
        /// </summary>
        [SerializeField]
        [Tooltip("Eyes of the actual VR player so it can be sync with the capsule. If you provided a _playerOrigin you must also provide a _playerEyes.")]
        private Transform _playerEyes;

        [SerializeField]
        private float moveSpeed = 10.0f;

        [SerializeField]
        private float rotateSnapAngle = 25.0f;


        private Func<float> _deltaTimeProvider = () => Time.deltaTime;
        public void SetDeltaTimeProvider(Func<float> deltaTimeProvider)
        {
            _deltaTimeProvider = deltaTimeProvider;
        }


        protected Action<LocomotionEvent, Pose> _whenLocomotionEventHandled = delegate { };
        public event Action<LocomotionEvent, Pose> WhenLocomotionEventHandled
        {
            add
            {
                _whenLocomotionEventHandled += value;
            }
            remove
            {
                _whenLocomotionEventHandled -= value;
            }
        }


        /// <summary>
        /// Indicates whether the character was detected as grounded after
        /// the last move.
        /// </summary>
        public bool IsGrounded => _characterController.IsGrounded;

        private Pose _accumulatedDeltaFrame;
        private Vector3 _velocity;


        //Distace from the Sellion to the Top of the Head.
        //Percentile 50 averaged between Male and Female
        private const float _sellionToTopOfHead = 0.1085f;

        //Half distace from the Sellion to the Back of the Head.
        //Percentile 50 averaged between Male and Female
        private const float _sellionToBackOfHeadHalf = 0.0965f;

        private Queue<LocomotionEvent> _deferredLocomotionEvent = new Queue<LocomotionEvent>();
        private YieldInstruction _endOfFrame = new WaitForEndOfFrame();
        private Coroutine _endOfFrameRoutine = null;

        protected bool _started;


        protected virtual void Start()
        {
            this.BeginStart(ref _started);

            this.AssertField(_characterController, nameof(_characterController));
            this.AssertField(_playerOrigin, nameof(_playerOrigin));
            this.AssertField(_playerEyes, nameof(_playerEyes));

            this.EndStart(ref _started);
        }

        protected virtual void OnEnable()
        {
            if (_started)
            {
                _endOfFrameRoutine = StartCoroutine(EndOfFrameCoroutine());
            }
        }

        protected virtual void OnDisable()
        {
            if (_started)
            {
                _accumulatedDeltaFrame = Pose.identity;
                StopCoroutine(_endOfFrameRoutine);
                _endOfFrameRoutine = null;
            }
        }

        protected virtual void Update()
        {
            CatchUpCharacterToPlayer();

            //UpdateVelocity();

            Pose startPose = _characterController.Pose;
            Vector3 delta = _velocity * _deltaTimeProvider.Invoke();
            _characterController.Move(delta);
            Pose endPose = _characterController.Pose;
            AccumulateDelta(ref _accumulatedDeltaFrame, startPose, endPose);

        }

        protected virtual void LateUpdate()
        {
            ConsumeDeferredLocomotionEvents();
        }

        public void MoveForward()
        {
            _velocity = _playerEyes.forward * moveSpeed;
        }

        public void MoveBackward()
        {
            _velocity = _playerEyes.forward * -moveSpeed;
        }

        public void MoveUp()
        {

            _velocity = Vector3.up * moveSpeed;
        }

        public void MoveDown()
        {
            _velocity = Vector3.up * -moveSpeed;
        }

        public void Stop()
        {
            _velocity = Vector3.zero;
        }

        private bool _hasSnapped = false;
        public void SnapRight()
        {
            if (!_hasSnapped)
            {
                HandleLocomotionEvent(new LocomotionEvent(0, Quaternion.AngleAxis(rotateSnapAngle, Vector3.up), LocomotionEvent.RotationType.Relative));
               _hasSnapped = true;
            }
        }

        public void SnapLeft()
        {
            if (!_hasSnapped)
            {
                HandleLocomotionEvent(new LocomotionEvent(0, Quaternion.AngleAxis(-rotateSnapAngle, Vector3.up), LocomotionEvent.RotationType.Relative));
                _hasSnapped = true;
            }
        }

        public void ResetSnap()
        {
            _hasSnapped = false;
        }

        public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
        {
            //Velocity translations get added directly (but will be applied during update)
            if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Velocity)
            {
                AddVelocity(locomotionEvent.Pose.position);
                _whenLocomotionEventHandled.Invoke(locomotionEvent, locomotionEvent.Pose);
            }
            else //other events are stored to be excuted at the end of the frame
            {
                if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute
                    || locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel
                    || locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
                {
                    _velocity = Vector3.zero;
                }
                _deferredLocomotionEvent.Enqueue(locomotionEvent);
            }
        }

        private void AddVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        private void ConsumeDeferredLocomotionEvents()
        {
            if (_deferredLocomotionEvent.Count == 0)
            {
                return;
            }

            Pose startPose = _characterController.Pose;
            while (_deferredLocomotionEvent.Count > 0)
            {
                LocomotionEvent locomotionEvent = _deferredLocomotionEvent.Dequeue();
                HandleDeferredLocomotionEvent(locomotionEvent);
            }
            Pose endPose = _characterController.Pose;
            AccumulateDelta(ref _accumulatedDeltaFrame, startPose, endPose);
        }

        private void HandleDeferredLocomotionEvent(LocomotionEvent locomotionEvent)
        {
            Pose startPose = _characterController.Pose;

            //Teleport
            if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute)
            {
                MoveAbsoluteFeet(locomotionEvent.Pose.position);
            }
            else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel)
            {
                MoveAbsoluteHead(locomotionEvent.Pose.position);
            }
            else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
            {
                MoveRelative(locomotionEvent.Pose.position);
            }

            //Rotation
            if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Absolute)
            {
                RotateAbsolute(locomotionEvent.Pose.rotation);
            }
            else if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Relative)
            {
                RotateRelative(locomotionEvent.Pose.rotation);
            }
            else if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Velocity)
            {
                RotateVelocity(locomotionEvent.Pose.rotation);
            }

            Pose endPose = _characterController.Pose;
            Pose delta = Pose.identity;
            AccumulateDelta(ref delta, startPose, endPose);
            _whenLocomotionEventHandled.Invoke(locomotionEvent, delta);
        }

        private void MoveAbsoluteFeet(Vector3 target)
        {
            //Move to target
            Vector3 characterFeet = GetCharacterFeet();
            Vector3 offset = target - characterFeet;
            Vector3 pos = _characterController.Pose.position + offset;
            _characterController.SetPosition(pos);
        }

        private void MoveAbsoluteHead(Vector3 target)
        {
            Vector3 characterHead = GetCharacterHead();
            Vector3 offset = target - characterHead;
            Vector3 pos = _characterController.Pose.position + offset;
            _characterController.SetPosition(pos);
        }

        private void MoveRelative(Vector3 offset)
        {
            _velocity = Vector3.zero;
            _characterController.Move(offset);
        }

        private void RotateAbsolute(Quaternion target)
        {
            _characterController.SetRotation(target);
        }

        private void RotateRelative(Quaternion target)
        {
            target = target * _characterController.Pose.rotation;
            _characterController.SetRotation(target);
        }

        private void RotateVelocity(Quaternion target)
        {
            target.ToAngleAxis(out float angle, out Vector3 axis);
            angle *= _deltaTimeProvider.Invoke();
            target = Quaternion.AngleAxis(angle, axis) * _characterController.Pose.rotation;
            _characterController.SetRotation(target);
        }


        private void AccumulateDelta(ref Pose accumulator, in Pose from, in Pose to)
        {
            accumulator.position = accumulator.position + to.position - from.position;
            accumulator.rotation = (Quaternion.Inverse(from.rotation) * to.rotation) * accumulator.rotation;
        }

        protected virtual void LastUpdate()
        {
            CatchUpPlayerToCharacter(_accumulatedDeltaFrame, GetCharacterFeet().y);
            _accumulatedDeltaFrame = Pose.identity;
        }

        private IEnumerator EndOfFrameCoroutine()
        {
            while (true)
            {
                yield return _endOfFrame;
                LastUpdate();
            }
        }

        private void CatchUpCharacterToPlayer()
        {
            Vector3 head = GetPlayerHead();
            Vector3 delta = head - _characterController.Pose.position;
            Vector3 forward = Vector3.ProjectOnPlane(_playerEyes.forward, Vector3.up);

            _characterController.Move(delta);
            _characterController.SetRotation(Quaternion.LookRotation(forward, Vector3.up));
        }

        private void CatchUpPlayerToCharacter(Pose delta, float feetHeight)
        {
            Pose characterPose = _characterController.Pose;
            Vector3 originalHeadPosition = GetPlayerHead();
            _playerOrigin.rotation = delta.rotation * _playerOrigin.rotation;
            _playerOrigin.position = _playerOrigin.position + originalHeadPosition - GetPlayerHead();
            Vector3 finalPosition = _playerOrigin.position + delta.position;
            _playerOrigin.position = finalPosition;
            _characterController.SetPosition(characterPose.position);
            _characterController.SetRotation(characterPose.rotation);

        }

        private Vector3 GetCharacterHead()
        {
            Vector3 characterHead = _characterController.Pose.position
                + Vector3.up * (_characterController.Height * 0.5f - _sellionToTopOfHead + _characterController.SkinWidth);
            return characterHead;
        }

        private Vector3 GetPlayerHead()
        {
            return _playerEyes.position - _playerEyes.forward * _sellionToBackOfHeadHalf;
        }

        private Vector3 GetCharacterFeet()
        {
            Vector3 characterFeet = _characterController.Pose.position
                + Vector3.down * (_characterController.Height * 0.5f + _characterController.SkinWidth);
            return characterFeet;
        }

        #region Inject

        public void InjectAllFlyingLocomotor(CharacterController characterController,
            Transform playerEyes, Transform playerOrigin)
        {
            InjectCharacterController(characterController);
            InjectPlayerEyes(playerEyes);
            InjectPlayerOrigin(playerOrigin);
        }

        public void InjectCharacterController(CharacterController characterController)
        {
            _characterController = characterController;
        }

        public void InjectPlayerEyes(Transform playerEyes)
        {
            _playerEyes = playerEyes;
        }

        public void InjectPlayerOrigin(Transform playerOrigin)
        {
            _playerOrigin = playerOrigin;
        }

        #endregion
    }

}
