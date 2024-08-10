using UnityEngine;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;

[System.Serializable]
public class StageRegistry
{
    public static StageRegistry Instance { get; private set; }
    public static Dictionary<StageEntity.ClassType, EntityCollection> EntityRegistry = new();
    public static Transform Parent => Instance._registryParent;

    #region  ================= [[ ENTITY COLLECTION ]] ================= >>
    [System.Serializable]
    public class EntityCollection
    {
        [SerializeField] Transform _collectionParent;
        [ShowOnly] public StageEntity.ClassType classType;
        [SerializeField, Range(1, 32)] int maxEntityCount = 1;

        [ShowOnly] public List<StageEntity> entities = new();

        public EntityCollection(StageEntity.ClassType classType, Transform parent = null)
        {
            this.classType = classType;

            if (parent == null)
                parent = StageRegistry.Parent;
            _collectionParent = parent;
        }

        public void AddEntity(StageEntity entity)
        {
            entities.Add(entity);
        }

        public void RemoveEntity(StageEntity entity)
        {
            entities.Remove(entity);
        }

        public bool IsCollectionFull()
        {
            return entities.Count >= maxEntityCount;
        }
    }
    public static EntityCollection GetEntityCollection(StageEntity.ClassType classType)
    {
        if (!EntityRegistry.ContainsKey(classType))
        {
            EntityRegistry.Add(classType, new EntityCollection(classType));
        }
        return EntityRegistry[classType];
    }

    #endregion

    // ----------------- Data -------------------


    // ----------------- Serialized Fields -------------------
    [SerializeField] Transform _registryParent;
    [SerializeField] List<EntityCollection> _entityCollections = new();

    public StageRegistry()
    {
        Instance = this;

        // Initialize the entity registry
        EntityRegistry = new Dictionary<StageEntity.ClassType, EntityCollection>
        {
            { StageEntity.ClassType.PLANE, new EntityCollection(StageEntity.ClassType.PLANE)},
            { StageEntity.ClassType.BLIMP, new EntityCollection(StageEntity.ClassType.BLIMP)},
            { StageEntity.ClassType.CLOUD, new EntityCollection(StageEntity.ClassType.CLOUD)}
        };

        // Update the list of entity collections
        _entityCollections = new List<EntityCollection>(EntityRegistry.Values);
    }

    public static bool IsCollectionFull(StageEntity.ClassType classType)
    {
        EntityCollection collection = GetEntityCollection(classType);
        return collection.IsCollectionFull();
    }

    public static void RegisterEntity(StageEntity entity)
    {
        EntityCollection collection = GetEntityCollection(entity.classType);

        entity.transform.parent = Instance._registryParent;
        collection.AddEntity(entity);
    }

}