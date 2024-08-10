using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using UnityEngine;
using NaughtyAttributes;
using System.Collections;
using System;
using Darklight.UnityExt.Editor;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour
{
    const string PREFIX = "[Spawner]";

    [System.Serializable]
    public class EntitySpawnSettings
    {
        public StageEntity.ClassType entityType;
        [Range(0, 100)] public int maxCount;
        [Range(0, 1)] public float spawnChance;
    }

    // ---------------- Data ----------------------
    Shape2D _shape2D;
    List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    Dictionary<StageEntity.ClassType, List<StageEntity>> _spawnedEntities = new();
    Coroutine _spawnRoutine;

    // ---------------- Serialized Data ----------------------
    [SerializeField, Expandable] Shape2DPreset _shape2DPreset;

    [HorizontalLine, Header("Live Data")]
    [SerializeField, ShowOnly] bool _active = false;

    [HorizontalLine, Header("Settings")]
    [SerializeField, Range(1, 10)] float _tickSpeed = 2;
    [SerializeField] SpawnPoint.State _spawnPoint_defaultState = SpawnPoint.State.AVAILABLE;

    [Header("Entity Settings")]
    public List<EntitySpawnSettings> _entitySpawnSettings = new List<EntitySpawnSettings>();


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

    #region --------- ( Handle Entities ) ---------
    T SpawnEntity<T>(Vector3 position) where T : StageEntity
    {
        // Get the class type enum
        StageEntity.ClassType classType = GetClassType<T>();

        // Check if we can spawn this entity
        if (!CanSpawnEntity(classType)) return null;

        // Create the entity
        T newEntity = Stage.Entities.CreateEntity<T>();
        newEntity.transform.position = position;

        // Add to the dictionary
        if (!_spawnedEntities.ContainsKey(newEntity.classType))
            _spawnedEntities.Add(newEntity.classType, new List<StageEntity>());
        _spawnedEntities[newEntity.classType].Add(newEntity);

        return newEntity;
    }

    T SpawnEntity<T>(SpawnPoint spawnPoint) where T : StageEntity
    {
        return SpawnEntity<T>(spawnPoint.position);
    }

    GameObject SpawnEntity(StageEntity.ClassType classType, SpawnPoint spawnPoint)
    {
        GameObject newEntity = null;
        switch (classType)
        {
            case StageEntity.ClassType.CLOUD:
                newEntity = Stage.Entities.CreateEntity<CloudEntity>().gameObject;
                break;
            case StageEntity.ClassType.PLANE:
                newEntity = Stage.Entities.CreateEntity<PlaneEntity>().gameObject;
                break;
            case StageEntity.ClassType.BLIMP:
                newEntity = Stage.Entities.CreateEntity<BlimpEntity>().gameObject;
                break;
        }

        // Set the position
        newEntity.transform.position = spawnPoint.position;

        return newEntity;
    }

    StageEntity.ClassType GetClassType<T>() where T : StageEntity
    {
        if (typeof(T) == typeof(CloudEntity)) return StageEntity.ClassType.CLOUD;
        if (typeof(T) == typeof(PlaneEntity)) return StageEntity.ClassType.PLANE;
        if (typeof(T) == typeof(BlimpEntity)) return StageEntity.ClassType.BLIMP;
        return StageEntity.ClassType.NULL;
    }

    int GetEntityCount(StageEntity.ClassType classType)
    {
        return _spawnedEntities.ContainsKey(classType) ? _spawnedEntities[classType].Count : 0;
    }

    bool CanSpawnEntity(StageEntity.ClassType classType)
    {
        foreach (EntitySpawnSettings settings in _entitySpawnSettings)
        {
            if (settings.entityType == classType)
            {
                return GetEntityCount(classType) < settings.maxCount;
            }
        }
        return false;
    }
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

            // Get random entity settings
            if (_entitySpawnSettings.Count == 0) continue;
            int randomIndex = UnityEngine.Random.Range(0, _entitySpawnSettings.Count);
            EntitySpawnSettings randomEntitySettings = _entitySpawnSettings[randomIndex];

            // Roll the dice to see if we should spawn this entity
            float randomChance = UnityEngine.Random.Range(0f, 1f);
            if (randomChance <= randomEntitySettings.spawnChance)
            {
                SpawnEntity(randomEntitySettings.entityType, randSpawnPoint);
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