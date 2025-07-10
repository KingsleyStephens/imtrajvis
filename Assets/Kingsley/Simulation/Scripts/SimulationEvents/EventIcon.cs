using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventIcon : MonoBehaviour
{
    public TextMeshPro label;
    public TextMeshPro subLabel;
    public float minScale = 1.0f;
    public float maxScale = 100.0f;
    public float maxDist = 100.0f;
    public float maxSubLabelDistScale = 0.25f;
    public bool billboarded = true;

    private CameraSwitcher cameraManager;

    public void SetCamera(GameObject camera)
    {
        GameObject camObj = GameObject.FindGameObjectWithTag("CAMERA_MANAGER");
        if (camObj != null)
        {
            cameraManager = camObj.GetComponent<CameraSwitcher>();
        }
    }

    public void SetColour(Color colour) { 
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.material.color = colour;
        }
    }
    public void SetLabel(string text) {
        if (label != null)
        {
            label.text = text;
        }
    }
    public void SetSubLabel(string text) {
        if (subLabel != null)
        {
            subLabel.text = text;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraManager != null)
        {
            Vector3 camPos = cameraManager.CurrentCamera.transform.position;

            if (billboarded)
            {
                transform.LookAt(camPos);
                transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
            }

            float dist = (camPos - transform.position).magnitude;
            if (dist < maxDist)
            {
                label.enabled = true;
                subLabel.enabled = dist < maxDist * maxSubLabelDistScale;
                
                float scale = Mathf.Lerp(minScale, maxScale, dist / maxDist);
                transform.localScale = new Vector3(scale, scale, scale);
            } 
            else
            {
                label.enabled = false;
                subLabel.enabled = false;
            }
        }
    }
}
