using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;

using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using Darklight.UnityExt.FMODExt;
using Darklight.UnityExt.Editor;



#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LocalPlayerInputManager), typeof(EntityRegistry))]
public class Stage : MonoBehaviourSingleton<Stage>
{
    public enum AreaType { ALL, STAGE, SPAWN_AREA }

    // ----------------- Static Properties -------------------
    public static StageData_Settings Settings => Instance._settings;

    // -------------- Data ------------------------
    Dictionary<LocalPlayerInputData, PlaneEntity> _players = new();


    [Header("Data")]
    [SerializeField, Expandable] StageData_Settings _settings;

    [Header("Spawners")]
    public Spawner spawner_innerStage;
    public Spawner spawner_middleStage;
    public Spawner spawner_outerStage;

    [Header("Wind")]
    [SerializeField, ShowOnly] float _targetWindRotation = 0f;
    [SerializeField, ShowOnly] float _targetWindIntensity = 0f;

    [SerializeField, ShowOnly] float _curr_windDirection = 0f;
    [SerializeField, ShowOnly] float _curr_windIntensity = 0f;

    // -------------- References ------------------------
    public Vector3 stageCenter => transform.position;
    public float stageRadius => _settings.stageRadius;



    #region ================= [[ UNITY METHODS ]] ================= >>
    public override void Initialize()
    {
        if (Application.isPlaying)
        {
            LocalPlayerInputManager.Instance.OnAddLocalPlayerInput += AssignPlayerToPlane;

            StartWindRoutine();
        }
    }

    public void Update()
    {
        // Update the wind direction
        _curr_windDirection = Mathf.Lerp(_curr_windDirection, _targetWindRotation, Time.deltaTime);
        _curr_windIntensity = Mathf.Lerp(_curr_windIntensity, _targetWindIntensity, Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        if (_settings == null) return;

        // Draw the stage center radius
        Shape2DGizmos.DrawCircle(transform.position, _settings.stageRadius, Color.red);

        // Draw the spawn radius
        Shape2DGizmos.DrawCircle(transform.position, _settings.spawnRadius, Color.yellow);

        // Draw the wind direction
        Gizmos.color = Color.white;
        Vector3 windDir = Quaternion.AngleAxis(_curr_windDirection, Vector3.up) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + windDir * stageRadius);
    }
    #endregion

    #region ================= [[ STAGE MANAGEMENT ]] ================= >>

    #region (( ---- Collider Handling ---- ))
    /// <summary>
    /// Returns an array of colliders within a given radius.
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    Collider[] GetCollidersInRadius(float radius)
    {
        return Physics.OverlapSphere(stageCenter, radius);
    }
    #endregion

    #region (( ---- Entity Handling ---- ))

    public List<T> GetAllEntitiesOfType<T>() where T : StageEntity
    {
        return FindObjectsByType<T>(FindObjectsSortMode.InstanceID).ToList();
    }
    #endregion

    #endregion

    #region ================= [[ PLAYER MANAGEMENT ]] ================= >>

    public void AssignPlayerToPlane(LocalPlayerInputData playerInputData)
    {
        // If the player is already assigned to a plane, return
        if (_players.ContainsKey(playerInputData)) return;

        // If no planes are available, spawn a new one
        PlaneEntity newPlane = spawner_innerStage.SpawnEntityAtRandomAvailablePoint<PlaneEntity>();
        if (newPlane != null)
            newPlane.AssignPlayerInput(playerInputData);

        if (playerInputData.device is Gamepad)
            LocalPlayerInputManager.Instance.RumbleGamepad((Gamepad)playerInputData.device, 0.5f, 0.5f);

        _players.Add(playerInputData, newPlane);
        Debug.Log($"{Prefix} Player {playerInputData.playerName} assigned to plane {newPlane.name}");
    }

    #endregion

    #region ================= [[ WIND MANAGEMENT ]] ================= >>

    public void StartWindRoutine()
    {
        StartCoroutine(WindRoutine());
    }

    IEnumerator WindRoutine()
    {
        while (true)
        {
            Debug.Log($"{Prefix} Changing wind direction and intensity");
            _targetWindRotation = Random.Range(0, 360);
            _targetWindIntensity = Random.Range(0, 100);

            yield return new WaitForSeconds(30f);
        }
    }

    #endregion

    #region ================= [[ STATIC METHODS ]] ================= >>

    /// <summary>
    /// Check if a position is within a given radius of the stage center.
    /// </summary>
    /// <param name="position">
    ///     The position to check.
    /// </param>
    /// <param name="radius">
    ///     The radius to check against.
    /// </param>
    /// <returns>
    ///     Returns true if the position is within the radius of the stage center.
    /// </returns>
    public static bool IsPositionWithinRadius(Vector3 position, float radius)
    {
        Vector3 stageCenter = Instance.stageCenter;
        return Vector3.Distance(stageCenter, position) <= radius;
    }

    public static bool IsPositionWithinRadiusRange(Vector3 position, float minRadius, float maxRadius)
    {
        Vector3 stageCenter = Instance.stageCenter;
        float distance = Vector3.Distance(stageCenter, position);
        return distance >= minRadius && distance <= maxRadius;
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(Stage))]
public class StageCustomEditor : Editor
{
    SerializedObject _serializedObject;
    Stage _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (Stage)target;
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
        }
    }
}
#endif