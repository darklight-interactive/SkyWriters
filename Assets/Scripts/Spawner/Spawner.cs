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
    const string PREFIX = "[Spawner]";

    // ---------------- Data ----------------------
    Shape2D _shape2D;
    List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    Coroutine _spawnRoutine;

    // ---------------- Serialized Data ----------------------
    [SerializeField, Expandable] Shape2DPreset _shape2DPreset;

    [HorizontalLine, Header("Settings")]
    [SerializeField, Range(1, 10)] float _tickSpeed = 2;
    [SerializeField] SpawnPoint.State _defaultState = SpawnPoint.State.AVAILABLE;


    [HorizontalLine, Header("Primary Points")]
    [SerializeField] SpawnPoint _primaryA;
    [SerializeField, Range(0, Shape2D.MAX_SEGMENTS)] int _primaryA_index = 0;
    [SerializeField, Range(0, Shape2D.MAX_SEGMENTS)] int _primaryA_neighborInfluence = 3;
    [SerializeField] SpawnPoint.State _primaryA_state = SpawnPoint.State.AVAILABLE;


    [HorizontalLine, Header("Gizmos")]
    public bool showGizmos = true;
    [Range(1, 100)] public int gimzoSize = 10;
    public Color gizmoColor = Color.gray;

    // ---------------- References ----------------------
    public bool active => _spawnRoutine == null;

    #region ================= [[ UNITY METHODS ]] ================= >>
    void Start()
    {
        Refresh();

        if (Application.isPlaying)
            BeginSpawnRoutine();
    }

    void OnDrawGizmos()
    {
        if (_shape2D != null) _shape2D.DrawGizmos(gizmoColor);

        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            spawnPoint.DrawGizmos(gimzoSize);
        }

    }
    #endregion

    #region ================= [[ BASE METHODS ]] ================= >>
    public void Refresh()
    {
        _shape2D = _shape2DPreset.CreateShapeAt(transform.position);
        GenerateSpawnPoints();

        if (_primaryA != null && _primaryA_index < _spawnPoints.Count)
        {
            _primaryA = _spawnPoints[_primaryA_index];
            _primaryA.GoToState(_primaryA_state);

            List<SpawnPoint> affectedNeighbors = GetSpawnPoint_Neighbors(_primaryA_index, _primaryA_neighborInfluence);
            SetPointsToState(affectedNeighbors, _primaryA_state);
        }


    }

    #region --------- ( Handle Spawn Points ) ---------
    void GenerateSpawnPoints()
    {
        _spawnPoints.Clear();
        for (int i = 0; i < _shape2D.vertices.Length; i++)
        {
            SpawnPoint newSpawnPoint = new SpawnPoint(i, _shape2D.vertices[i]);
            _spawnPoints.Add(newSpawnPoint);
        }
    }

    #region ---- << Getters >> ----
    public SpawnPoint GetSpawnPoint_ClosestTo(Vector3 position)
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

    public List<SpawnPoint> GetSpawnPoint_Neighbors(int index, int count)
    {
        List<SpawnPoint> neighbors = new List<SpawnPoint>();
        for (int i = 1; i < count; i++)
        {
            int rightIndex = index + i;
            if (rightIndex >= 0 && rightIndex <= _spawnPoints.Count)
            {
                neighbors.Add(_spawnPoints[rightIndex]);
            }

            int leftIndex = index - i;
            if (leftIndex <= 0) leftIndex = _spawnPoints.Count + leftIndex;
            if (leftIndex >= 0 && leftIndex < _spawnPoints.Count)
            {
                neighbors.Add(_spawnPoints[leftIndex]);
            }
        }
        return neighbors;
    }

    public List<SpawnPoint> GetAllSpawnPoints_InState(SpawnPoint.State state)
    {
        List<SpawnPoint> points = new List<SpawnPoint>();
        foreach (SpawnPoint point in _spawnPoints)
        {
            if (point.CurrentState == state)
            {
                points.Add(point);
            }
        }
        return points;
    }

    public SpawnPoint GetSpawnPoint_RandomInState(SpawnPoint.State state)
    {
        List<SpawnPoint> points = GetAllSpawnPoints_InState(state);
        if (points.Count == 0) return null;
        return points[Random.Range(0, points.Count)];
    }

    #endregion

    #region ---- << Setters >> ----

    public void SetPointsToState(List<SpawnPoint> points, SpawnPoint.State state)
    {
        foreach (SpawnPoint point in points)
        {
            point.GoToState(state);
        }
    }

    #endregion

    #endregion

    #region --------- ( Handle Entities ) ---------
    public T SpawnEntityAt<T>(Vector3 position) where T : StageEntity
    {
        T newEntity = Stage.Entities.CreateEntity<T>();
        newEntity.transform.position = position;
        return newEntity;
    }

    public T SpawnEntityAt<T>(SpawnPoint spawnPoint) where T : StageEntity
    {
        return SpawnEntityAt<T>(spawnPoint.position);
    }
    #endregion


    #endregion



    #region ================= [[ SPAWNER ROUTINE ]] ================= >>
    public void BeginSpawnRoutine()
    {
        if (active) return;
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void EndSpawnRoutine()
    {
        if (_spawnRoutine == null) return;
        StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    IEnumerator SpawnRoutine()
    {
        Debug.Log($"{PREFIX} Spawn Routine Started");
        int tickCount = 0;

        yield return new WaitForSeconds(1);

        while (active)
        {
            yield return new WaitForSeconds(_tickSpeed);
            tickCount++;

            SpawnPoint randSpawnPoint = GetSpawnPoint_RandomInState(SpawnPoint.State.AVAILABLE);

            /*
            CloudEntity newCloud = StageManager.Instance.SpawnEntity<CloudEntity>(randSpawnPosition);
            newCloud.SetTargetRotation(StageManager.WindDirection, true);
            */

            if (tickCount % 3 == 0)
            {
                BlimpEntity newBlimp = SpawnEntityAt<BlimpEntity>(randSpawnPoint);
                newBlimp.SetTargetRotation(Stage.Settings.windDirection, true);
            }
        }
    }
    #endregion





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

        _script.Refresh();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        if (GUILayout.Button("Toggle Spawn Routine"))
        {
            if (_script.active)
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
            _script.Refresh();
        }
    }
}
#endif