using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityExplosion : MonoBehaviour
{
    public float explodeRadius = 10.0f;
    public int explodeDamage = 500;
    public GameObject explosionPrefab;

    private GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        target = gameObject.GetComponent<Target>()?.TargetGameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            float dist = (target.transform.position - transform.position).magnitude;
            if (dist < explodeRadius)
            {
                Health h = target.gameObject.GetComponent<Health>();
                if (h != null) {
                    h.TakeDamage((int)((float)explodeDamage * (1.0f - dist / explodeRadius)));

                    if (explosionPrefab != null && EventManager.Instance.Visible)
                    {
                      Instantiate(explosionPrefab, transform.position, transform.rotation);
                    }

                    EventManager.Instance?.AddEvent(new EventManager.Event(
                        target.GetComponent<SquadFSMAgent>().Id,
                        EventManager.EventType.Destroyed,
                        EventManager.EventSubType.Begin,
                        "Agent destroyed: " + transform.position.ToString(),
                        transform.position, transform.forward));


                    gameObject.SetActive(false);
                    //Destroy(gameObject);
                }
            }
        }
    }
}
