using System;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{

    public List<int> list = new List<int>();
    public List<Vector3> hits = new List<Vector3>();
    private LineRenderer lineRenderer;
    private bool raysActive;
    private RaySwitch raySwitch;

    // Use this for initialization
    void Start()
    {
        raysActive = false;
        raySwitch = FindObjectOfType<RaySwitch>();
    }

    // Update is called once per frame
    public void Update()
    {
        list.Clear();
        hits.Clear();
        lineRenderer = GetComponent<LineRenderer>();

        for (var i = -135; i < 136; i++)
        {
            var downRay = new Ray(transform.position, Quaternion.Euler(0, i, 0) * transform.forward);

            // Cast a ray straight downwards.
            if (Physics.Raycast(downRay, out var hit))
            {
                list.Add(Math.Min(10000, (int)(hit.distance * 1000)));
                hits.Add(hit.point);
                hits.Add(transform.position);
            }
            else
            {
                list.Add(10000);
            }
        }
        
        if (lineRenderer != null)
        {
            lineRenderer.SetPositions(hits.ToArray());
            lineRenderer.positionCount = raysActive && raySwitch.RaysActive ? hits.Count : 0;
        }
    }

    public void ToggleSensorRays()
    {
        raysActive = !raysActive;
    }

    private void OnDrawGizmos()
    {
        if (raySwitch != null && !raySwitch.RaysActive || !raysActive)
            return;

        Gizmos.color = Color.red;
        for (var i = -135; i < 136; i++)
        {
            var downRay = new Ray(transform.position, Quaternion.Euler(0, i, 0) * transform.forward);

            // Cast a ray straight downwards.
            if (Physics.Raycast(downRay, out var hit))
            {
                // Gizmos.DrawLine(transform.position, hit.point);
            }
        }
    }
}


