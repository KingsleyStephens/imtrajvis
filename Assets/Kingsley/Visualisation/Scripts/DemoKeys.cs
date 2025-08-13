using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace KVisualisation
{

    public class DemoKeys : MonoBehaviour
    {
        [Serializable]
        public class KeyEvent
        {
            public InputAction action;
            public UnityEvent callback;
        }

        public KeyEvent[] keyEvents;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            foreach (var ke in keyEvents)
            {
                ke.action.performed += (ctx) => ke.callback.Invoke();
                ke.action.Enable();
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }

}
