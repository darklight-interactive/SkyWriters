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
    [SerializeField, Range(0, 5)] int _spawnEntryWidthMultiplier = 2;
    [SerializeField, Range(0, 100)] float _gizmoSize = 10;
    [SerializeField] List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

    [SerializeField] private SpawnPoint _windEntryPoint; // Opposite of the wind direction angle
    [SerializeField] private SpawnPoint _windExitPoint; // In the direction of the wind direction angle

    public override void Initialize()
    {
        CreateSpawnPoints();

	if (Application.isPlaying)
		BeginSpawnRoutine();

    }

    void Update()
    {
        // Cache the wind direction and opposite direction to avoid recalculating
        float windDirection = StageManager.WindDirection;
        float oppositeWindDirection = windDirection + 180;

        // Get the spawn points
        SpawnPoint newWindExitPoint = GetClosestSpawnPointInDirection(windDirection);
        SpawnPoint newWindEntryPoint = GetClosestSpawnPointInDirection(oppositeWindDirection);

        // Update wind exit point only if it has changed
        if (_windExitPoint.position != newWindExitPoint.position)
        {
            _windExitPoint?.GoToState(SpawnPoint.State.WAITING);
            newWindExitPoint?.GoToState(SpawnPoint.State.DISABLED);
            _windExitPoint = newWindExitPoint;
        }

        // Update wind entry point only if it has changed
        if (_windEntryPoint.position != newWindEntryPoint.position)
        {
            _windEntryPoint?.GoToState(SpawnPoint.State.WAITING);
            newWindEntryPoint?.GoToState(SpawnPoint.State.SPAWNING);
            _windEntryPoint = newWindEntryPoint;
        }

        List<SpawnPoint> remainder = new List<SpawnPoint>(_spawnPoints);
        List<SpawnPoint> entryNeighbors = GetSpawnPointNeighbors(_windEntryPoint.index, _spawnEntryWidthMultiplier);
        List<SpawnPoint> exitNeighbors = GetSpawnPointNeighbors(_windExitPoint.index, _spawnEntryWidthMultiplier);
        foreach (SpawnPoint neighbor in entryNeighbors)
        {
            neighbor.GoToState(SpawnPoint.State.SPAWNING);
        }

        foreach (SpawnPoint neighbor in exitNeighbors)
        {
            neighbor.GoToState(SpawnPoint.State.DISABLED);
        }


        // Apply the waiting state to the remaining spawn points
        remainder.Remove(_windEntryPoint);
        remainder.Remove(_windExitPoint);
        remainder.RemoveAll(entryNeighbors.Contains);
        remainder.RemoveAll(exitNeighbors.Contains);
        foreach (SpawnPoint point in remainder)
        {
            point.GoToState(SpawnPoint.State.WAITING);
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

        Debug.Log($"{Prefix} Created {_spawnPointCount} Spawn Points");
    }

    List<SpawnPoint> GetSpawnPointNeighbors(int index, int count)
    {
        List<SpawnPoint> neighbors = new List<SpawnPoint>();
        for (int i = 1; i < count; i++)
        {
            int rightNeighborIndex = (index + i) % _spawnPoints.Count;
            if (rightNeighborIndex < _spawnPoints.Count && rightNeighborIndex >= 0)
            {
                neighbors.Add(_spawnPoints[rightNeighborIndex]);
            }

            int leftNeighborIndex = (index - i) % _spawnPoints.Count;
            if (leftNeighborIndex < _spawnPoints.Count && leftNeighborIndex >= 0)
            {
                try
                {
                    neighbors.Add(_spawnPoints[leftNeighborIndex]);
                }
                catch (System.Exception e)
                {
                    Debug.Log($"{Prefix} : leftNeighborIndex ({leftNeighborIndex}) Error: {e.Message}");
                }
            }

        }
        return neighbors;
    }

    List<SpawnPoint> GetAvailableSpawnPoints()
    {
        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            if (spawnPoint.CurrentState == SpawnPoint.State.SPAWNING)
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }
        return availableSpawnPoints;
    }

    SpawnPoint GetRandomAvailableSpawnPoint()
    {
        List<SpawnPoint> availableSpawnPoints = GetAvailableSpawnPoints();
        if (availableSpawnPoints.Count == 0) return null;
        return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
    }

    public void BeginSpawnRoutine()
    {
        if (_spawnRoutineActive) return;

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
        int tickCount = 0;

        yield return new WaitForSeconds(1);

        while (_spawnRoutineActive)
        {
            yield return new WaitForSeconds(_tickSpeed);
            tickCount++;

            Vector3 randSpawnPosition = GetRandomAvailableSpawnPoint().position;

            /*
            CloudEntity newCloud = StageManager.Instance.SpawnEntity<CloudEntity>(randSpawnPosition);
            newCloud.SetTargetRotation(StageManager.WindDirection, true);
            */

            if (tickCount % 3 == 0)
            {
                BlimpEntity newBlimp = StageManager.Instance.SpawnEntity<BlimpEntity>(randSpawnPosition);
                newBlimp.SetTargetRotation(StageManager.WindDirection, true);
            }
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

        if (GUILayout.Button("Initialize"))
        {
            _script.Initialize();
        }

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
        }
    }
}
#endif