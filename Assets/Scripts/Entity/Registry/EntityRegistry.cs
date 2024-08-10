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
        return (T)CreateNewEntity(typeof(T), customSettings);
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

    public static StageEntity CreateNewEntity(Type entityType, EntitySettings customSettings = null)
    {
        // Ensure the type is a subclass of StageEntity
        if (!typeof(StageEntity).IsAssignableFrom(entityType))
        {
            Debug.LogError($"{Prefix} Cannot create entity because {entityType} is not a valid StageEntity type");
            return null;
        }

        // Find the collection for this entity type
        EntityCollection collection = GetEntityCollection(entityType);
        if (collection != null && collection.IsCollectionFull())
        {
            Debug.LogWarning($"{Prefix} Cannot create new entity of type {entityType} because the collection is full");
            return null;
        }

        // Use custom settings if provided, otherwise, find the default settings for this entity type
        EntitySettings settings = customSettings ?? GetSettingsFromType(entityType);
        if (settings == null)
        {
            Debug.LogError($"{Prefix} No settings found for entity type {entityType}");
            return null;
        }

        // Get the prefab from the settings
        GameObject prefab = settings.prefab;
        if (prefab == null)
        {
            Debug.LogError($"{Prefix} No prefab found in settings for entity type {entityType}");
            return null;
        }

        // Instantiate the prefab & initialize with preset settings
        GameObject entityObj = UnityEngine.Object.Instantiate(prefab);
        StageEntity newEntity = (StageEntity)entityObj.GetComponent(entityType);
        if (newEntity != null)
        {
            newEntity.Initialize(settings);

            // Register the entity
            AddToRegistry(newEntity);
        }
        else
        {
            Debug.LogError($"{Prefix} Failed to get component of type {entityType} from instantiated prefab");
            UnityEngine.Object.Destroy(entityObj);
            return null;
        }

        return newEntity;
    }

    public static StageEntity CreateNewEntity(Type entityType, Vector3 position, EntitySettings customSettings = null)
    {
        StageEntity newEntity = CreateNewEntity(entityType, customSettings);
        if (newEntity != null)
        {
            newEntity.transform.position = position;
        }
        return newEntity;
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

    public static EntityCollection GetEntityCollection(StageEntity.Class classType)
    {
        if (!_registry.ContainsKey(classType))
        {
            _registry.Add(classType, new EntityCollection(classType));
        }
        return _registry[classType];
    }

    public static EntityCollection GetEntityCollection(Type entityType)
    {
        StageEntity.Class classType = GetClassFromType(entityType);
        return GetEntityCollection(classType);
    }

    public static EntityCollection GetEntityCollection<T>() where T : StageEntity
    {
        StageEntity.Class classType = GetClassFromType<T>();
        return GetEntityCollection(classType);
    }

    public static EntitySettings GetSettingsFromClass(StageEntity.Class classType)
    {
        switch (classType)
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

    public static EntitySettings GetSettingsFromType(Type entityType)
    {
        StageEntity.Class classType = GetClassFromType(entityType);
        return GetSettingsFromClass(classType);
    }

    public static EntitySettings GetSettingsFromType<T>() where T : StageEntity
    {
        StageEntity.Class classType = GetClassFromType<T>();
        return GetSettingsFromClass(classType);
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