using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CloudInteractable : MonoBehaviour
{
    SphereCollider _collider => GetComponent<SphereCollider>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlaneController>())
        {
            StageManager.Instance.SpawnRandomCloud();
            Destroy(gameObject);
        }
    }
}
