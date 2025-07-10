using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    private int numTorpedos = 10;

    public GameObject torpedoPrefab;

    public bool HasAmmo { get { return numTorpedos > 0; } }

    public void Fire(GameObject target)
    {
        if (HasAmmo)
        {
            GameObject torperdoGO = Instantiate(torpedoPrefab, transform.position, transform.rotation);
            MoveableObject moveable = torperdoGO.GetComponent<MoveableObject>();
            if (moveable != null)
            {
                moveable.InitMoveableObject(transform.position, transform.rotation);
            }

            Target targetComp = torperdoGO.GetComponent<Target>();
            if (targetComp != null)
            {
                targetComp.TargetGameObject = target;
            }

            if (!EventManager.Instance.Visible)
            {
                MeshRenderer mr = torperdoGO.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    mr.enabled = false;
                }

                TrailRenderer tr = torperdoGO.GetComponent<TrailRenderer>();
                if (tr != null)
                {
                    tr.enabled = false;
                }
            }


            numTorpedos--;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
