using SATestbed;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThreeDSim", menuName = "ThreeDSim", order = 1)]
public class ThreeDSim : Simulation
{
    [SerializeField]
    public Environment environment;
    [SerializeField]
    public Updateable[] managers;

    /**
     * Init
     */
    public override void Init()
    {
        SATestbed.Logger.LogMessage("ThreeDSim LOAD");
        Enabled = true;
    }

    /**
     * Begin
    */
    public override void BeginUpdating()
    {
        SATestbed.Logger.LogMessage("ThreeDSim BEGIN");
        if (environment != null)
        {
            ExecutionManager.Instance.Add(environment);
        }

        foreach (var manager in managers)
        {
            ExecutionManager.Instance.Add(manager);
        }
    }

    /**
     * End
     */
    public override void EndUpdating()
    {
        SATestbed.Logger.LogMessage("ThreeDSim END");
    }

    protected override void DoUpdate(TimeSnapshot time)
    {
        //SATestbed.Logger.LogMessage("ThreeDSim update t:{0}, dt:{1}", time.time, time.deltaTime);
    }


}
