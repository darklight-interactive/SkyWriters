using UnityEngine;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;
using System;

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
    }

    static void RemoveFromRegistry(StageEntity entity)
    {
        EntityCollection collection = GetEntityCollection(entity.entityClass);
        collection.RemoveEntity(entity);
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
            _registry.Add(entityClass, new EntityCollection(entityClass));
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
    [Expandable, SerializeField] CloudEntitySettings _cloudSettings;
    [Expandable, SerializeField] PlaneEntitySettings _planeSettings;
    [Expandable, SerializeField] BlimpEntitySettings _blimpSettings;

    // ================= [[ UNITY METHODS ]] ================= >>
    public override void Initialize()
    {

    }

}