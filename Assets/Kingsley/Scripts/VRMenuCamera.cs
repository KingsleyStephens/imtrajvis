using UnityEngine;

namespace Oculus.Interaction.Locomotion
{

    public class VRMenuCamera : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField]
        private CharacterController _characterController;

        private Vector3 mInitPos;
        private Quaternion mInitRot;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_characterController != null)
            {
                mInitPos = _characterController.transform.position;
                mInitRot = _characterController.transform.rotation;
            }
        }

        private void OnEnable()
        {
            if (_characterController != null)
            {
                _characterController.transform.position = mInitPos;
                _characterController.transform.rotation = mInitRot;
            }
        }
    }

}