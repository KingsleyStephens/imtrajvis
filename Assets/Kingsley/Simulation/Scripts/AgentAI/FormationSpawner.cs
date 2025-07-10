using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FormationSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject agentPrefab;
    
    [SerializeField]
    private int agentCount = 10;

    [SerializeField]
    private GameObject formationPrefab;

    [SerializeField]
    private Color colour = Color.white;

    private bool mHasSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!mHasSpawned)
        {
            Spawn();
        }
    }

    public GameObject Spawn()
    {
        GameObject formation = Instantiate(formationPrefab, transform.position, transform.rotation);
        MoveableObject moveable = formation.GetComponent<MoveableObject>();
        if (moveable != null)
        {
            moveable.InitMoveableObject(transform.position, transform.rotation);
        }

        Formation formationComponent = formation.GetComponent<Formation>();
        if (formationComponent != null)
        {
            for (int i = 0; i < agentCount; i++)
            {
                GameObject agent = null;
                FormationSpawner spawner = agentPrefab.GetComponent<FormationSpawner>();
                if (spawner != null)
                {
                    agent = spawner.Spawn();
                }
                else
                {
                    agent = Instantiate(agentPrefab, formationComponent.GetFormationPos(i), transform.rotation);
                }

                MoveableObject agentMoveable = agent.GetComponent<MoveableObject>();
                if (agentMoveable != null)
                {
                    agentMoveable.InitMoveableObject(agent.transform.position, agent.transform.rotation);
                }

                formationComponent.Squad.Add(agent);
            }

            formationComponent.SetFormation(formationComponent.CurrentFormation, colour);
        }

        mHasSpawned = true;
        return formation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
