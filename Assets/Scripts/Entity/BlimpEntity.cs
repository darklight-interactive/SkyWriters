using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Darklight.UnityExt.Behaviour;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlimpEntity : StageEntity
{
    private float _cloudSpawnDelay = 10f;
    private Vector3 _exhaustPosition
    {
        get
        {
            Vector3 output = transform.position + (transform.forward * data.colliderHeight * -1);
            return output;
        }
    }

    public class BlimpStateMachine : StateMachine
    {
        public BlimpStateMachine(StageEntity entity) : base(entity)
        {
            this.entity = entity; // set the entity in the base class

            possibleStates = new Dictionary<State, FiniteState<State>>();
            AddState(new BlimpSpawnState(this, State.SPAWN));
        }
    }

    public class BlimpSpawnState : StateMachine.SpawnState
    {
        public BlimpSpawnState(StageEntity.StateMachine stateMachine, State stateType) : base(stateMachine, stateType) { }
        public override void Enter()
        {
            base.Enter();
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
            //StageManager.Instance.SpawnCloudAt(_exhaustPosition, VFX_Manager.ColorPalette.blueColor);

        }
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(_exhaustPosition, data.colliderRadius * 0.5f);
    }

}