using UnityEngine;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class EntityRegistry : MonoBehaviourSingleton<EntityRegistry>
{
    // ----------------- Static Data -------------------
    static Dictionary<StageEntity.Class, EntityCollection> _registry = new();

    #region ================= [[ STATIC METHODS ]] ================= >>

    #region ENTITY CREATION

    public static T CreateNewEntity<T>(EntitySettings customSettings = null) where T : StageEntity
    {
        // Find the collection for this entity type
        StageEntity.Class entityClass = GetClassFromType<T>();
        EntityCollection collection = GetEntityCollection(entityClass);
        if (collection != null && collection.IsCollectionFull())
        {
            Debug.LogWarning($"{Prefix} Cannot create new entity of type {entityClass} because the collection is full");
            return null;
        }

        // Use custom settings if provided, otherwise, find the default settings for this entity type
        EntitySettings settings = customSettings ?? GetDefaultSettingsForClass(entityClass);
        if (settings == null)
        {
            Debug.LogError($"{Prefix} No settings found for entity type {entityClass}");
            return null;
        }

        // Get the prefab from the settings
        GameObject prefab = settings.prefab;
        if (prefab == null)
        {
            Debug.LogError($"{Prefix} No prefab found in settings for entity type {entityClass}");
            return null;
        }

        // Instantiate the prefab & initialize with preset settings
        GameObject entityObj = UnityEngine.Object.Instantiate(prefab);
        StageEntity newEntity = entityObj.GetComponent<T>();

        if (newEntity != null)
        {
            newEntity.Initialize(settings);

            // Register the entity
            AddToRegistry(newEntity);
        }
        else
        {
            Debug.LogError($"{Prefix} Failed to get component of type {entityClass} from instantiated prefab");
            UnityEngine.Object.Destroy(entityObj);
            return null;
        }

        return (T)newEntity;
    }

    public static T CreateNewEntity<T>(Vector3 position, EntitySettings customSettings = null) where T : StageEntity
    {
        T newEntity = CreateNewEntity<T>(customSettings);
        if (newEntity != null)
        {
            newEntity.transform.position = position;
        }
        return newEntity;
    }

    public static StageEntity CreateNewEntity(StageEntity.Class entityClass, EntitySettings customSettings = null)
    {
        if (entityClass == StageEntity.Class.NULL)
        {
            Debug.LogError($"{Prefix} Cannot create entity of class NULL");
            return null;
        }

        // Use custom settings if provided, otherwise, find the default settings for this entity type
        EntitySettings settings = customSettings;
        switch (entityClass)
        {
            case StageEntity.Class.CLOUD:
                return CreateNewEntity<CloudEntity>(settings);
            case StageEntity.Class.PLANE:
                return CreateNewEntity<PlaneEntity>(settings);
            case StageEntity.Class.BLIMP:
                return CreateNewEntity<BlimpEntity>(settings);
            default:
                return null;
        }
    }

    #endregion

    #region (( ENTITY REGISTRATION ))
    static void AddToRegistry(StageEntity entity)
    {
        EntityCollection collection = GetEntityCollection(entity.entityClass);
        collection.AddEntity(entity);

        ClearNullEntities();
    }

    public static void RemoveFromRegistry(StageEntity entity)
    {
        EntityCollection collection = GetEntityCollection(entity.entityClass);
        collection.RemoveEntity(entity);

        ClearNullEntities();
    }

    public static void RemoveFromRegistryWithDelay(StageEntity entity, float delay)
    {
        Instance.StartCoroutine(Instance.RemoveFromRegistryWithDelayRoutine(entity, delay));
    }

    IEnumerator RemoveFromRegistryWithDelayRoutine(StageEntity entity, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveFromRegistry(entity);
    }

    static void ClearNullEntities()
    {
        foreach (KeyValuePair<StageEntity.Class, EntityCollection> pair in _registry)
        {
            pair.Value.entities.RemoveAll(entity => entity == null);
        }
    }

    #endregion

    #region (( GETTER METHODS ))
    public static Type GetTypeFromClass(StageEntity.Class classEnum)
    {
        switch (classEnum)
        {
            case StageEntity.Class.CLOUD:
                return typeof(CloudEntity);
            case StageEntity.Class.PLANE:
                return typeof(PlaneEntity);
            case StageEntity.Class.BLIMP:
                return typeof(BlimpEntity);
            default:
                return null;
        }
    }

    public static StageEntity.Class GetClassFromType<T>() where T : StageEntity
    {
        return GetClassFromType(typeof(T));
    }

    public static StageEntity.Class GetClassFromType(Type entityType)
    {
        if (!typeof(StageEntity).IsAssignableFrom(entityType))
        {
            return StageEntity.Class.NULL;
        }

        switch (entityType)
        {
            case Type t when t == typeof(CloudEntity):
                return StageEntity.Class.CLOUD;
            case Type t when t == typeof(PlaneEntity):
                return StageEntity.Class.PLANE;
            case Type t when t == typeof(BlimpEntity):
                return StageEntity.Class.BLIMP;
            default:
                return StageEntity.Class.NULL;
        }
    }

    public static EntityCollection GetEntityCollection(StageEntity.Class entityClass)
    {
        if (!_registry.ContainsKey(entityClass))
        {
            _registry.Add(entityClass, new EntityCollection(entityClass, 32));
        }
        return _registry[entityClass];
    }
    public static EntityCollection GetEntityCollection<T>() where T : StageEntity
    {
        StageEntity.Class classType = GetClassFromType<T>();
        return GetEntityCollection(classType);
    }

    public static EntitySettings GetDefaultSettingsForClass(StageEntity.Class entityClass)
    {
        switch (entityClass)
        {
            case StageEntity.Class.CLOUD:
                return Instance._cloudSettings;
            case StageEntity.Class.PLANE:
                return Instance._planeSettings;
            case StageEntity.Class.BLIMP:
                return Instance._blimpSettings;
            default:
                return null;
        }
    }

    public static EntitySettings GetSettingsFromType<T>() where T : StageEntity
    {
        StageEntity.Class classType = GetClassFromType<T>();
        return GetDefaultSettingsForClass(classType);
    }

    #endregion

    public static bool IsEntityRegistered(StageEntity entity)
    {
        EntityCollection collection = GetEntityCollection(entity.entityClass);
        return collection.Contains(entity);
    }

    #endregion

    // ----------------- Serialized Data -------------------
    [Header("Clouds")]
    [Expandable, SerializeField] CloudEntitySettings _cloudSettings;
    [SerializeField] EntityCollection _cloudCollection;

    [Header("Planes")]
    [Expandable, SerializeField] PlaneEntitySettings _planeSettings;
    [SerializeField] EntityCollection _planeCollection;

    [Header("Blimps")]
    [Expandable, SerializeField] BlimpEntitySettings _blimpSettings;
    [SerializeField] EntityCollection _blimpCollection;

    // ================= [[ UNITY METHODS ]] ================= >>
    public override void Initialize()
    {
        _registry.Clear();
        _registry.Add(StageEntity.Class.CLOUD, new EntityCollection(StageEntity.Class.CLOUD, 999));
        _registry.Add(StageEntity.Class.PLANE, new EntityCollection(StageEntity.Class.PLANE, 8));
        _registry.Add(StageEntity.Class.BLIMP, new EntityCollection(StageEntity.Class.BLIMP, 2));

        UpdateCollections();
    }

    public void Update()
    {
        UpdateCollections();
    }

    void UpdateCollections()
    {
        _cloudCollection = GetEntityCollection<CloudEntity>();
        _planeCollection = GetEntityCollection<PlaneEntity>();
        _blimpCollection = GetEntityCollection<BlimpEntity>();
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(EntityRegistry))]
public class EntityRegistryCustomEditor : Editor
{
    SerializedObject _serializedObject;
    EntityRegistry _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (EntityRegistry)target;
        _script.Awake();
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

