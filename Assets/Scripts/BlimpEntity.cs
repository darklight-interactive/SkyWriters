using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlimpEntity : StageEntity
{
    [SerializeField] private float _cloudSpawnDelay = 2f;

    IEnumerator SpawnClouds()
    {
        while (true)
        {
            yield return new WaitForSeconds(_cloudSpawnDelay);
            StageManager.Instance.SpawnCloudAt(transform.position);
        }
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(BlimpEntity))]
public class BlimpEntityCustomEditor : StageEntityCustomEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

}
#endif