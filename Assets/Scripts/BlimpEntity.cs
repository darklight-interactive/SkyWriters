using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlimpEntity : StageEntity
{
    [SerializeField] private float _cloudSpawnDelay = 2f;
    [SerializeField] private float _cloudDeathDelay = 1f;
    private Vector3 _exhaustPosition
    {
        get
        {
            Vector3 output = CalculateTargetPosition(currentPosition, -90, _colliderHeight);
            return output;
        }
    }

    public override void Start()
    {
        base.Start();
        StartCoroutine(SpawnExhaustClouds());
    }

    IEnumerator SpawnExhaustClouds()
    {
        while (true)
        {
            yield return new WaitForSeconds(_cloudSpawnDelay);
            StageManager.Instance.SpawnCloudAt(_exhaustPosition);
        }
    }

    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(_exhaustPosition, _colliderRadius * 0.5f);
    }

}