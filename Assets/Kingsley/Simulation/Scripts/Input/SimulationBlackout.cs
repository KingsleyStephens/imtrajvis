using UnityEngine;
using UnityEngine.InputSystem;
using KCore;

namespace KSimulation
{ 
    [CreateAssetMenu(fileName = "SimulationBlackout", menuName = "SimulationBlackout", order = 1)]
    public class SimulationBlackout : Updateable
    {
        private bool isBlackout = false;

        /**
            * Init
            */
        public override void Init()
        {
            KCore.Logger.LogMessage("SimulationBlackout LOAD");
            Enabled = true;
        }

        /**
            * Begin
        */
        public override void BeginUpdating()
        {
            KCore.Logger.LogMessage("SimulationBlackout BEGIN");
        }

        /**
            * End
            */
        public override void EndUpdating()
        {
            KCore.Logger.LogMessage("SimulationBlackout END");
        }

        protected override void DoUpdate(TimeSnapshot time) 
        {
            // Logger.LogMessage("SimulationBlackout update t:{0}, dt:{1}", time.time, time.deltaTime);
            var keyboard = Keyboard.current;
            if (keyboard == null) 
            {
                return; // No keyboard connected.
            }

            if (keyboard.spaceKey.wasPressedThisFrame) 
            {
                isBlackout = !isBlackout;

                SetVisibility(!isBlackout);
            }
        }

        private void SetVisibility(bool visible)
        {
            EventManager.Instance.Visible = visible;
            EventManager.Instance.narrativeManager.Visible = visible;

            foreach (var a in AgentManager.Instance.Agents)
            {
                a.Visible = visible;
            }

            foreach (var f in AgentManager.Instance.Formations)
            {
                f.Visible = visible;
            }

        }
    }
}