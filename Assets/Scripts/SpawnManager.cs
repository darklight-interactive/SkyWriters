using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpawnManager : MonoBehaviourSingleton<SpawnManager>
{

    public class SpawnPoint
    {
        public Vector3 position;

        public SpawnPoint(Vector3 position)
        {
            this.position = position;
        }
    }

    [SerializeField, Range(4, 24)] int _spawnPointCount = 8;
    [SerializeField, Range(0, 100)] float _gizmoSize = 10;
    [SerializeField] List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();


    public override void Initialize()
    {
        Vector3 _stageCenter = StageManager.StageCenter;

        // Calculate the radius at the midpoint between the stage radius and the spawn radius offset
        float _spawnRadius = StageManager.StageRadius + (StageManager.SpawnRadiusOffset * 0.5f);

        // Calculate the positions of the spawn points
        List<Vector3> positions = StageManager.CalculatePointsInCircle(_stageCenter, _spawnRadius, _spawnPointCount, Vector3.up);

        // Create the spawn points
        _spawnPoints.Clear();
        for (int i = 0; i < _spawnPointCount; i++)
        {
            SpawnPoint spawnPoint = new SpawnPoint(positions[i]);
            _spawnPoints.Add(spawnPoint);
        }
    }

    void OnDrawGizmos()
    {
        if (_spawnPoints == null) return;

        Gizmos.color = Color.yellow;
        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint.position, _gizmoSize);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpawnManager))]
public class SpawnManagerCustomEditor : Editor
{
    SerializedObject _serializedObject;
    SpawnManager _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (SpawnManager)target;
        _script.Initialize();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Initialize();
        }
    }
}
#endif