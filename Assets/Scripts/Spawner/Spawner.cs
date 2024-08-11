using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;
using NaughtyAttributes;
using System.Collections;
using System;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour
{
    const string PREFIX = "[Spawner]";
    public ConsoleGUI guiConsole { get; private set; } = new ConsoleGUI();

    // ---------------- Data ----------------------
    List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    Coroutine _spawnRoutine;

    // ---------------- Serialized Data ----------------------
    [SerializeField, Expandable] SpawnerPreset _settings;

    [HorizontalLine, Header("Live Data")]
    [SerializeField, ShowOnly] bool _active = false;

    [HorizontalLine, Header("Settings")]
    [SerializeField, Range(1, 10)] float _tickSpeed = 2;
    [SerializeField] float _spawnDelay = 0.5f;
    [SerializeField] SpawnPoint.State _spawnPoint_defaultState = SpawnPoint.State.AVAILABLE;

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
    public bool active => _active;
    public float spawnDelay => _spawnDelay;
    public SpawnPoint.State spawnPoint_defaultState => _spawnPoint_defaultState;

    #region ================= [[ UNITY METHODS ]] ================= >>
    void Start()
    {


        Initialize();

        if (Application.isPlaying)
            BeginSpawnRoutine();
    }

    void OnDrawGizmos()
    {

        foreach (SpawnPoint spawnPoint in _spawnPoints)
        {
            spawnPoint.DrawGizmos(gimzoSize);
        }

    }
    #endregion



    #region ================= [[ BASE METHODS ]] ================= >>
    public void Initialize()
    {
        if (_settings == null)
        {
            Debug.LogError($"{PREFIX} No SpawnerPreset assigned", this);
            return;
        }
        // Create the shape2D object
        GenerateSpawnPoints();
        if (_primaryA != null && _primaryA_index < _spawnPoints.Count)
        {
            _primaryA = _spawnPoints[_primaryA_index];
            _primaryA.GoToState(_primaryA_state);

            List<SpawnPoint> affectedNeighbors = GetSpawnPoint_Neighbors(_primaryA_index, _primaryA_neighborInfluence);
            SetPointsToState(affectedNeighbors, _primaryA_state);
        }
    }

    public StageEntity SpawnEntityAtPoint(StageEntity.Class entityClass, SpawnPoint spawnPoint)
    {
        if (spawnPoint == null) return null;
        spawnPoint.GoToState(SpawnPoint.State.SPAWNING);

        StageEntity entity = EntityRegistry.CreateNewEntity(entityClass);
        return entity;
    }

    public T SpawnEntityAtPoint<T>(SpawnPoint spawnPoint) where T : StageEntity
    {
        StageEntity.Class entityClass = EntityRegistry.GetClassFromType(typeof(T));
        return (T)SpawnEntityAtPoint(entityClass, spawnPoint);
    }

    public T SpawnEntityAtRandomAvailablePoint<T>() where T : StageEntity
    {
        SpawnPoint spawnPoint = GetSpawnPoint_RandomInState(SpawnPoint.State.AVAILABLE);
        return SpawnEntityAtPoint<T>(spawnPoint);
    }

    #region --------- ( Handle Spawn Points ) ---------
    void GenerateSpawnPoints()
    {
        _spawnPoints.Clear();

        Shape2D shape2D = _settings.CreateShape2D();
        Vector3[] vertices = shape2D.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            SpawnPoint newSpawnPoint = new SpawnPoint(this, i, vertices[i]);
            _spawnPoints.Add(newSpawnPoint);
        }
    }

    public void GoToStateWitDelay(SpawnPoint spawnPoint, SpawnPoint.State state, float delay)
    {
        StartCoroutine(GoToStateWithDelayRoutine(spawnPoint, state, delay));
    }

    IEnumerator GoToStateWithDelayRoutine(SpawnPoint spawnPoint, SpawnPoint.State state, float delay)
    {
        yield return new WaitForSeconds(delay);
        spawnPoint.GoToState(state);
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
        return points[UnityEngine.Random.Range(0, points.Count)];
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



    #endregion

    #region ================= [[ SPAWNER ROUTINE ]] ================= >>
    public void BeginSpawnRoutine()
    {
        if (_active) return;
        _active = true;

        _spawnRoutine = StartCoroutine(SpawnRoutine());

        Debug.Log($"{PREFIX} Spawn Routine Started", this);
    }

    public void EndSpawnRoutine()
    {
        if (!_active) return;
        _active = false;

        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        Debug.Log($"{PREFIX} Spawn Routine Ended", this);
    }

    IEnumerator SpawnRoutine()
    {
        int tickCount = 0;
        while (_active)
        {
            yield return new WaitForSeconds(_tickSpeed);
            tickCount++;

            // Get a random spawn point
            SpawnPoint randSpawnPoint = GetSpawnPoint_RandomInState(SpawnPoint.State.AVAILABLE);
            if (randSpawnPoint == null) continue;

            // Get random entity settings
            EntitySettings randomEntitySettings = _settings.GetRandomEntitySettings();
            if (randomEntitySettings == null) continue;

            // Spawn the entity
            StageEntity.Class entityClass = randomEntitySettings.data.entityClass;
            SpawnEntityAtPoint(entityClass, randSpawnPoint);
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

        _script.Initialize();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        _script.guiConsole.DrawInEditor();

        CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Initialize();
        }
    }
}
#endif