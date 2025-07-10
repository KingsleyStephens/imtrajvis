using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int health = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Agent a = gameObject.GetComponent<SquadFSMAgent>();
            if (a != null)
            {
                Formation f = gameObject.GetComponent<FormationFollower>()?.mFormation;
                if (f != null && gameObject != null)
                {
                    f.RemoveSquadMember(gameObject);
                }

                Debug.LogWarning("Removing " + a.Id);

                AgentManager.Instance?.RemoveAgent(a);
            } else
            {
                Debug.LogWarning("AGENT NULL");
            }

            // Destroy(gameObject);
            gameObject.SetActive(false);
        }
    }

}
