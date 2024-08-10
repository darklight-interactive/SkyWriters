using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;
using NaughtyAttributes;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour
{
    const string Prefix = "[Spawner]";

    // ---------------- Data ----------------------
    Coroutine _spawnRoutine;
    List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

    // ---------------- Serialized Data ----------------------
    [SerializeField, Expandable] Shape2DPreset _shape2DPreset;
    [SerializeField] Shape2D _shape2D;


    [SerializeField] bool _spawnRoutineActive = false;
    [SerializeField, Range(1, 10)] float _tickSpeed = 2;


    // ---------------- References ----------------------
    public bool IsSpawnRoutineActive => _spawnRoutineActive;

    #region ================= [[ UNITY METHODS ]] ================= >>
    void Start()
    {
        if (Application.isPlaying)
            BeginSpawnRoutine();
    }

    void OnDrawGizmos()
    {
        if (_shape2D != null)
        {
            Shape2DGizmos.DrawShape2D(_shape2D, Color.green);
            Debug.Log($"{Prefix} Drawing Shape2D Gizmos");
        }
    }

    #endregion

    #region ================= [[ SHAPE2D ]] ================= >>
    public void RefreshShape()
    {
        _shape2D = _shape2DPreset.CreateShapeAt(transform.position);
    }


    #endregion


    #region ================= [[ SPAWN POINTS ]] ================= >>





    #endregion

    void AssignPointsToState(List<SpawnPoint> points, SpawnPoint.State state)
    {
        foreach (SpawnPoint point in points)
        {
            point.GoToState(state);
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
                BlimpEntity newBlimp = SpawnEntityAt<BlimpEntity>(randSpawnPosition);
                newBlimp.SetTargetRotation(Stage.Settings.windDirection, true);
            }
        }
    }

    T SpawnEntityAt<T>(Vector3 position) where T : StageEntity
    {
        T newEntity = Stage.Entities.CreateEntity<T>();
        newEntity.transform.parent = transform;
        newEntity.transform.position = position;
        return newEntity;
    }



}


#if UNITY_EDITOR
[CustomEditor(typeof(Spawner))]
public class SpawnManagerCustomEditor : Editor
{
    SerializedObject _serializedObject;
    Spawner _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (Spawner)target;

        _script.RefreshShape();
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
            _script.RefreshShape();
        }
    }
}
#endif