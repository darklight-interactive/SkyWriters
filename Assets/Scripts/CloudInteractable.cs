using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(Collider), typeof(ParticleSystem))]
public class CloudInteractable : MonoBehaviour
{
    ParticleSystem _particleSystem => GetComponent<ParticleSystem>();
    CloudGradientData _cloudParticleData;

    [SerializeField] float _radius = 1.0f;
    [SerializeField] float _speed = 10.0f;

    public void SetCloudData(CloudGradientData cloudParticleData)
    {
        _cloudParticleData = cloudParticleData;
        SetColorOverLifetime(_cloudParticleData.ToGradient());
    }

    void SetColorOverLifetime(Gradient gradient)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _particleSystem.colorOverLifetime;
        colorOverLifetime.color = gradient;
    }

    // ================== Unity Events ==================
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlaneController>())
        {
            StageManager.Instance.SpawnRandomCloud();

            // Set the contrail color to the cloud's color
            PlaneController planeController = other.gameObject.GetComponent<PlaneController>();

            if (planeController == null)
            {
                Debug.LogError("PlaneController not found on the plane object.");
                return;
            }

            if (_cloudParticleData == null)
            {
                Debug.LogError("CloudParticleData not set on the cloud object.");
                Destroy(gameObject);
                return;
            }

            planeController.CreateNewContrail(_cloudParticleData.ToGradient());
            Destroy(gameObject);
        }
    }

    // =================== Unity Methods ======================
    void Update()
    {
        // Move the cloud to the right
        transform.position += Vector3.right * _speed * Time.deltaTime;

        // Destroy the cloud if it goes off screen
        if (StageManager.Instance.IsColliderInStage(GetComponent<Collider>()) == false)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(gameObject);
#endif
            }
        }
    }
}
