using SATestbed;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZeroDSim", menuName = "ZeroDSim", order = 1)]
public class ZeroDSim : Simulation
{
    [SerializeField]
    public Environment environment;

    /**
     * Init
     */
    public override void Init()
    {
        SATestbed.Logger.LogMessage("ZeroDSim LOAD");
        Enabled = true;
    }

    /**
     * Begin
    */
    public override void BeginUpdating()
    {
        SATestbed.Logger.LogMessage("ZeroDSim BEGIN");
        if (environment != null)
        {
            ExecutionManager.Instance.Add(environment);
        }
    }

    /**
     * End
     */
    public override void EndUpdating()
    {
        SATestbed.Logger.LogMessage("ZeroDSim END");
    }

    protected override void DoUpdate(TimeSnapshot time)
    {
        //SATestbed.Logger.LogMessage("ZeroDSim update t:{0}, dt:{1}", time.time, time.deltaTime);
    }


}
