using KVisualisation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

namespace Oculus.Interaction.Locomotion
{
    public class VROrbitCamera : MonoBehaviour, ILocomotionEventHandler, IDeltaTimeConsumer, IVRControllerInputs
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
        private GameObject _pivotMarkerPrefab;



        [SerializeField]
        private float moveSpeed = 10.0f;

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

        private Transform pivot;
        private Transform camPosDummy;


        private int _orbitDir = 0;
        private int _zoomDir = 0;
        private int _raiseDir = 0;
        private Vector2 _panDir = new Vector2(0.0f, 0.0f);


        [SerializeField]
        private float orbitDistance = 40.0f;
        [SerializeField]
        private float orbitHeight = 40.0f;

        [SerializeField]
        private float minZoom = 1.0f;
        [SerializeField]
        private float maxZoom = 100.0f;


        [SerializeField]
        private float orbitSpeed = 10.0f;
        [SerializeField]
        private float zoomSpeed = 10.0f;
        [SerializeField]
        private float panSpeed = 10.0f;
        [SerializeField]
        private float verticleSpeed = 10.0f;

        private float mOrbitDist = 0.0f;
        private float mOrbitHeight = 0.0f;

        private bool mPanning = false;

        public bool Panning { get { return mPanning; } set { mPanning = value; } }

        public void TogglePanning()
        {
            mPanning = !mPanning;
        }

        public Vector3 PivotPos { get { return (pivot != null) ? pivot.position : Vector3.zero; } set { if (pivot != null) { pivot.transform.position = value; UpdateCameraPos(); } } }

        public void ParentPivotPos(Vector3 pos, Transform parent)
        {
            if (pivot == null)
            {
                if (_pivotMarkerPrefab != null)
                {
                    GameObject go = Instantiate(_pivotMarkerPrefab, parent);
                    pivot = go.transform;
                    pivot.localScale = Vector3.one;

                    GameObject goOrbit = new GameObject("camPosDummy");
                    camPosDummy = goOrbit.transform;
                    camPosDummy.parent = pivot;
                    camPosDummy.localScale = Vector3.one;
                }
            }

            pivot.localPosition = pos;
            pivot.localRotation = Quaternion.identity;
            mOrbitDist = orbitDistance;
            mOrbitHeight = orbitHeight;
            UpdateCameraPos();
        }

        private void UpdateCameraPos()
        {
            camPosDummy.localPosition = Vector3.zero;
            camPosDummy.localRotation = Quaternion.identity;
            camPosDummy.Translate(-camPosDummy.forward * mOrbitDist, Space.World);
            camPosDummy.Translate(Vector3.up * mOrbitHeight * mOrbitDist / orbitDistance, Space.World);
            camPosDummy.LookAt(pivot.position);

            _playerOrigin.position = camPosDummy.position;
            _playerOrigin.rotation = Quaternion.Euler(0.0f, camPosDummy.rotation.eulerAngles.y, 0.0f);
        }

        public void StickUp()
        {
            if (mPanning)
            {
                PanUp();
            }
            else
            {
                ZoomIn();
            }
        }

        public void StickDown()
        {
            if (mPanning)
            {
                PanDown();
            }
            else
            {
                ZoomOut();
            }
        }


        public void StickRight()
        {
            if (mPanning)
            {
                PanRight();
            }
            else
            {
                OrbitRight();
            }
        }

        public void StickLeft()
        {
            if (mPanning)
            {
                PanLeft();
            }
            else
            {
                OrbitLeft();
            }
        }


        public void StickRelease()
        {
            StopPan();
            StopZoom();
            StopOrbit();
        }


        public void ZoomIn()
        {
            Zoom(-1);
        }

        public void ZoomOut()
        {
            Zoom(1);
        }

        public void StopZoom()
        {
            Zoom(0);
        }


        public void Zoom(int direction)
        {
            _zoomDir = direction;
        }

        private bool _hasOrbitSnapped = false;

        public void OrbitRight()
        {
            Orbit(-1);
        }

        public void OrbitLeft()
        {
            Orbit(1);
        }

        public void StopOrbit()
        {
            Orbit(0);
            _hasOrbitSnapped = false;
        }


        public void Orbit(int direction)
        {
           _orbitDir = direction;
        }


        public void PanRight()
        {
            Pan(new Vector2(-1.0f, 0.0f));
        }

        public void PanLeft()
        {
            Pan(new Vector2(1.0f, 0.0f));
        }

        public void PanUp()
        {
            Pan(new Vector2(0.0f, 1.0f));
        }

        public void PanDown()
        {
            Pan(new Vector2(0.0f, -1.0f));
        }
        public void StopPan()
        {
            Pan(new Vector2(0.0f, 0.0f));
        }

        public void Pan(Vector2 pan)
        {
            _panDir = pan;
        }

        private bool _hasVerticalSnapped = false;

        public void RaiseUp()
        {
            Raise(1);
        }

        public void RaiseDown()
        {
            Raise(-1);
        }

        public void StopRaise()
        {
            Raise(0);
            _hasVerticalSnapped = false;
        }


        public void Raise(int direction)
        {
            _raiseDir = direction;
        }


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

            if (pivot != null)
            {
                pivot.gameObject.SetActive(true);
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

            if (pivot != null)
            {
                pivot.gameObject.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            CatchUpCharacterToPlayer();

            //UpdateVelocity();

            //Pose startPose = _characterController.Pose;

            if (pivot != null && camPosDummy != null && (_panDir != Vector2.zero || _orbitDir != 0 || _zoomDir != 0 || _raiseDir != 0))
            {
                float dt = _deltaTimeProvider.Invoke();
                mOrbitDist += _zoomDir * zoomSpeed * dt;
                mOrbitDist = Mathf.Clamp(mOrbitDist, minZoom, maxZoom);

                if (_orbitDir != 0 && !_hasOrbitSnapped)
                {
                    pivot.Rotate(Vector3.up, _orbitDir * orbitSpeed);
                    _hasOrbitSnapped = true;
                }
                
                Vector3 dummyForward = camPosDummy.forward;
                dummyForward.y = 0.0f;
                dummyForward.Normalize();
                Vector3 dummyRight = Vector3.Cross(dummyForward, Vector3.up);

                pivot.Translate((dummyRight * _panDir.x + dummyForward * _panDir.y).normalized * panSpeed * dt, Space.World);

                if (_raiseDir != 0 && !_hasVerticalSnapped)
                {
                    mOrbitHeight += _raiseDir * verticleSpeed;
                    float heightRange = orbitHeight * orbitDistance / mOrbitDist;
                    mOrbitHeight = Mathf.Clamp(mOrbitHeight, -heightRange, heightRange);
                    _hasVerticalSnapped = true;
                }

                UpdateCameraPos();
            }

            //Vector3 delta = _velocity * _deltaTimeProvider.Invoke();
            //_characterController.Move(delta);


            //Pose endPose = _characterController.Pose;
            //AccumulateDelta(ref _accumulatedDeltaFrame, startPose, endPose);

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


        // INTERFACE

        public void LeftThumbClick()
        {
            TogglePanning();
        }

        public void LeftThumbUp(float stickValue)
        {
            StickUp();
        }

        public void LeftThumbDown(float stickValue)
        {
            StickDown();
        }

        public void LeftThumbRight(float stickValue)
        {
            StickRight();
        }

        public void LeftThumbLeft(float stickValue)
        {
            StickLeft();
        }

        public void LeftThumbNeutral()
        {
            StickRelease();
        }

        public void LeftPrimaryButtonDown()
        {
            RaiseUp();
        }

        public void LeftPrimaryButtonUp()
        {
            StopRaise();
        }

        public void LeftSecondaryButtonDown()
        {
            RaiseDown();
        }

        public void LeftSecondaryButtonUp()
        {
            StopRaise();
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
