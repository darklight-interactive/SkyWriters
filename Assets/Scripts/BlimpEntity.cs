using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlimpEntity : StageEntity
{
    [SerializeField] private float _cloudSpawnDelay = 2f;

    public override void Start()
    {
        base.Start();
        StartCoroutine(SpawnClouds());
    }

    IEnumerator SpawnClouds()
    {
        while (true)
        {
            yield return new WaitForSeconds(_cloudSpawnDelay);
            StageManager.Instance.SpawnCloudAt(transform.position);
        }
    }

}