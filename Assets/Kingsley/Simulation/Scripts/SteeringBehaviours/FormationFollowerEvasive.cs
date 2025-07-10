using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationFollowerEvasive : Arrive
{
    public Formation mFormation;
    private int mSlotId;

    protected override void InitSteeringBehaviour()
    {
        base.InitSteeringBehaviour();
    }

    public void InitFollower(Formation formation, Color colour)
    {
        if (mMoveableObject == null)
        {
            mMoveableObject = GetComponent<MoveableObject>();
        }

        mFormation = formation;
        mSlotId = mFormation.RequestSlot(mMoveableObject.Position);

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.color = colour;
            if (meshRenderer.material.HasProperty("_WireColor"))
            {
                meshRenderer.material.SetColor("_WireColor", colour);
            }
        }
    }

    protected override void UpdateSteeringBehaviour() 
    {
        if (mFormation != null && mSlotId > -1)
        {
            mTarget.Position = mFormation.GetSlotPosition(mSlotId);
        }
    }


    public override Vector3 Calculate()
    {
        return base.Calculate();
    }
}
