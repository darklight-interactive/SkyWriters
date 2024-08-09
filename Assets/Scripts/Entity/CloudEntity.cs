using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(VFX_Manager))]
public class CloudEntity : StageEntity
{
    VFX_Manager _particleSystemHandler;

    // ================== Unity Events ==================
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlaneEntity>())
        {
            // Set the contrail color to the cloud's color
            PlaneEntity planeController = other.gameObject.GetComponent<PlaneEntity>();

            if (planeController == null)
            {
                Debug.LogError("PlaneController not found on the plane object.");
                return;
            }

            //planeController.CreateNewContrail(_colorGradient);
            Destroy(gameObject);
        }
    }
}
