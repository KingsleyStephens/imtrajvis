using UnityEngine;

public class ViewpointUI : MonoBehaviour
{
    public GameObject objectCentric;
    public GameObject personCentric;

    private int mViewPoint = 1;

    public int ViewPoint { set
        {
            mViewPoint = value;
            if (mViewPoint == 0)
            {
                if (objectCentric != null)
                {
                    objectCentric.SetActive(false);
                }

                if (personCentric != null)
                {
                    personCentric.SetActive(true);
                }
            }
            else
            {
                if (objectCentric != null)
                {
                    objectCentric.SetActive(true);
                }

                if (personCentric != null)
                {
                    personCentric.SetActive(false);
                }
            }
        }
    }

    public GameObject ObjectSelected { get; set; } = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ViewPoint = 1;
    }

    private void Update()
    {
        if (objectCentric != null)
        {
            objectCentric.SetActive(mViewPoint == 1 && ObjectSelected != null);
        }
    }

}
