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
            GameObject torpeodoGO = Instantiate(torpedoPrefab, transform.position, transform.rotation);
            torpeodoGO.name = gameObject.name + "_torpeodo_" + numTorpedos.ToString();
            MoveableObject moveable = torpeodoGO.GetComponent<MoveableObject>();
            if (moveable != null)
            {
                moveable.InitMoveableObject(transform.position, transform.rotation);
            }

            Target targetComp = torpeodoGO.GetComponent<Target>();
            if (targetComp != null)
            {
                targetComp.TargetGameObject = target;
            }

            if (!EventManager.Instance.Visible)
            {
                MeshRenderer mr = torpeodoGO.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    mr.enabled = false;
                }

                TrailRenderer tr = torpeodoGO.GetComponent<TrailRenderer>();
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
