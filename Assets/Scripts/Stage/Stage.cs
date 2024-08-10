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


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LocalPlayerInputManager))]
public class Stage : MonoBehaviourSingleton<Stage>
{
    public enum AreaType { ALL, STAGE, SPAWN_AREA }

    // ----------------- Static Properties -------------------
    public static StageData_Settings Settings => Instance._settings;
    public static StageData_Entities Entities => Instance._entities;


    // -------------- Data ------------------------
    Dictionary<LocalPlayerInputData, PlaneEntity> _players = new();
    [SerializeField, Expandable] StageData_Settings _settings;
    [SerializeField, Expandable] StageData_Entities _entities;

    // -------------- References ------------------------
    public Vector3 stageCenter => transform.position;
    public float stageRadius => _settings.stageRadius;
    public float windDirection => _settings.windDirection;
    public float windIntensity => _settings.windIntensity;


    #region ================= [[ UNITY METHODS ]] ================= >>
    public override void Initialize()
    {
        if (Application.isPlaying)
        {
            LocalPlayerInputManager.Instance.OnAddLocalPlayerInput += AssignPlayerToPlane;

            FMOD_EventManager.Instance.PlaySceneBackgroundMusic("MainScene");
            //FMOD_EventManager.Instance.PlayStartInteractionEvent();
        }
    }

    void OnDrawGizmos()
    {
        if (_settings == null) return;

        // Draw the stage radius
        //CustomGizmos_DrawCircle(stageCenter, stageRadius, Vector3.up, Color.green);

        // Draw the spawn area
        //CustomGizmos_DrawCircle(transform.position, _stageRadius + _spawnRadiusOffset, Vector3.up, Color.yellow);

        // Draw the wind direction
        Gizmos.color = Color.white;
        Vector3 windDir = Quaternion.AngleAxis(windDirection, Vector3.up) * Vector3.forward;
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

        /*
        // Find the first available plane
        List<PlaneEntity> planes = GetAllEntitiesOfType<PlaneEntity>();
        foreach (PlaneEntity plane in planes)
        {
            if (plane.IsAutopilot)
            {
                plane.AssignPlayerInput(playerInputData);
                return;
            }
        }
        */

        // If no planes are available, spawn a new one
        //PlaneEntity newPlane = SpawnEntity<PlaneEntity>();
        //newPlane.AssignPlayerInput(playerInputData);
        //_players.Add(playerInputData, newPlane);

        //Debug.Log($"{Prefix} Player {playerInputData.playerName} assigned to plane {newPlane.name}");
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
