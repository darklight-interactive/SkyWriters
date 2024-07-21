using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayTarget : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_ScreenPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

    [SerializeField] private Material _xray_material;
    [SerializeField] private Camera _camera;
    public LayerMask layerMask;

    // Update is called once per frame
    void Update()
    {
        /*
        Vector3 dir = _camera.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);
        if (Physics.Raycast(ray, 3000, layerMask))
        {
            _xray_material.SetFloat(SizeID, 1f);
        }
        else
        {
            _xray_material.SetFloat(SizeID, 0f);
        }
        */

        Vector3 view = _camera.WorldToViewportPoint(transform.position);
        _xray_material.SetVector(PosID, view);

    }
}
