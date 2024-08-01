using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;
using NaughtyAttributes;
using System.Collections;



#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SpawnManager : MonoBehaviourSingleton<SpawnManager>
{
    public static float SpawnRadius
    {
        get
        {
            return StageManager.StageRadius + (StageManager.SpawnRadiusOffset * 0.5f);
        }
    }

    private Coroutine _spawnRoutine;


    [SerializeField] bool _spawnRoutineActive = false;
    public bool IsSpawnRoutineActive => _spawnRoutineActive;

    [SerializeField, Range(1, 10)] float _tickSpeed = 2;

    [SerializeField, Range(4, 24)] int _spawnPointCount = 8;
    [SerializeField, Range(0, 100)] float _gizmoSize = 10;
    [SerializeField] List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

    [SerializeField] private SpawnPoint _windEntryPoint; // Opposite of the wind direction angle
    [SerializeField] private SpawnPoint _windExitPoint; // In the direction of the wind direction angle

    void OnValidate()
    {
        Initialize();

    }

    [Button]
    public override void Initialize()
    {
        CreateSpawnPoints();
    }

    void Update()
    {
        SpawnPoint oldWindExitPoint = _windExitPoint;
        SpawnPoint oldWindEntryPoint = _windEntryPoint;

        // Get the spawn point in the direction of the wind
        _windExitPoint = GetClosestSpawnPointInDirection(StageManager.WindDirection);
        if (oldWindExitPoint.position != _windExitPoint.position)
        {
            oldWindExitPoint?.GoToState(SpawnPoint.State.WAITING);
        }
        _windExitPoint?.GoToState(SpawnPoint.State.DISABLED);

        // Get the opposite spawn point to the wind exit point
        _windEntryPoint = GetClosestSpawnPointInDirection(StageManager.WindDirection + 180);
        if (oldWindEntryPoint.position != _windEntryPoint.position)
        {
            oldWindEntryPoint?.GoToState(SpawnPoint.State.WAITING);
        }
        _windEntryPoint?.GoToState(SpawnPoint.State.SPAWNING);


        List<SpawnPoint> entry_neighbors = GetSpawnPointNeighbors(_windEntryPoint.index, 4);
        foreach (SpawnPoint neighbor in entry_neighbors)
        {
            neighbor.GoToState(SpawnPoint.State.SPAWNING);
        }

        List<SpawnPoint> exit_neighbors = GetSpawnPointNeighbors(_windExitPoint.index, 4);
        foreach (SpawnPoint neighbor in exit_neighbors)
        {
            neighbor.GoToState(SpawnPoint.State.DISABLED);
        }
    }

    void OnDrawGizmos()
    {
        if (_spawnPoints == null) return;
        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            Gizmos.color = spawnPoint.GetColor();
            Gizmos.DrawSphere(spawnPoint.position, _gizmoSize);
        }
    }

    public SpawnPoint GetClosestSpawnPointTo(Vector3 position)
    {
        SpawnPoint closestSpawnPoint = null;
        float closestDistance = float.MaxValue;

        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            float distance = Vector3.Distance(position, spawnPoint.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpawnPoint = spawnPoint;
            }
        }

        return closestSpawnPoint;
    }

    public SpawnPoint GetAntipodalSpawnPointTo(Vector3 position)
    {
        SpawnPoint antipodalSpawnPoint = null;
        float furthestDistance = float.MinValue;

        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            float distance = Vector3.Distance(position, spawnPoint.position);
            if (distance > furthestDistance)
            {
                furthestDistance = distance;
                antipodalSpawnPoint = spawnPoint;
            }
        }

        return antipodalSpawnPoint;
    }

    public SpawnPoint GetClosestSpawnPointInDirection(float angle)
    {
        Vector3 center = StageManager.StageCenter;
        float radius = SpawnRadius;
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        Vector3 targetPosition = center + direction * radius;
        return GetClosestSpawnPointTo(targetPosition);
    }

    public SpawnPoint GetRandomSpawnPointOfState(SpawnPoint.State state)
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            if (spawnPoint.CurrentState == state)
            {
                spawnPoints.Add(spawnPoint);
            }
        }

        if (spawnPoints.Count == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    void CreateSpawnPoints()
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
            SpawnPoint spawnPoint = new SpawnPoint(i, positions[i]);
            _spawnPoints.Add(spawnPoint);
        }
    }

    List<SpawnPoint> GetSpawnPointNeighbors(int index, int count)
    {
        List<SpawnPoint> neighbors = new List<SpawnPoint>();
        for (int i = 0; i < count; i++)
        {
            int rightNeighborIndex = (index + i) % _spawnPoints.Count;
            int leftNeighborIndex = (index - i) % _spawnPoints.Count;
            neighbors.Add(_spawnPoints[rightNeighborIndex]);
            neighbors.Add(_spawnPoints[leftNeighborIndex]);
        }
        return neighbors;
    }

    public void BeginSpawnRoutine()
    {
        _spawnRoutineActive = true;
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void EndSpawnRoutine()
    {
        Debug.Log($"{Prefix} Spawn Routine Ended");

        _spawnRoutineActive = false;
        if (_spawnRoutine == null) return;

        StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    IEnumerator SpawnRoutine()
    {
        Debug.Log($"{Prefix} Spawn Routine Started");

        while (_spawnRoutineActive)
        {
            yield return new WaitForSeconds(_tickSpeed);




            Vector3 spawnPosition = _windEntryPoint.position;
            CloudEntity newCloud = StageManager.Instance.SpawnEntity<CloudEntity>(spawnPosition);
            newCloud.SetTargetRotation(StageManager.WindDirection, true);
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
        _script.Awake();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        if (GUILayout.Button("Toggle Spawn Routine"))
        {
            if (_script.IsSpawnRoutineActive)
            {
                _script.EndSpawnRoutine();
            }
            else
            {
                _script.BeginSpawnRoutine();
            }
        }


        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Initialize();
        }
    }
}
#endif