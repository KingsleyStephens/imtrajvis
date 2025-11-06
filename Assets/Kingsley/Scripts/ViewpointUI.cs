using UnityEngine;

public class ViewpointUI : MonoBehaviour
{
    public GameObject toggleButton;


    public GameObject ObjectSelected { get; set; } = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void Update()
    {
        if (toggleButton != null)
        {
            toggleButton.SetActive(ObjectSelected != null);
        }
    }

}
