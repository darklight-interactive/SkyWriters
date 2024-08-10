using UnityEngine;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;

[System.Serializable]
public class StageRegistry
{
    public static StageRegistry Instance { get; private set; }
    public static Dictionary<StageEntity.ClassType, EntityCollection> EntityRegistry = new();

    #region  ================= [[ ENTITY COLLECTION ]] ================= >>
    [System.Serializable]
    public class EntityCollection
    {
        [ShowOnly] public StageEntity.ClassType classType;
        [SerializeField, Range(1, 32)] int maxEntityCount = 1;

        [ShowOnly] public List<StageEntity> entities = new();

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
            EntityRegistry.Add(classType, new EntityCollection() { classType = classType });
        }
        return EntityRegistry[classType];
    }

    #endregion

    // ----------------- Data -------------------


    // ----------------- Serialized Fields -------------------
    [SerializeField] List<EntityCollection> _entityCollections = new();

    public StageRegistry()
    {
        Instance = this;

        // Initialize the entity registry
        EntityRegistry = new Dictionary<StageEntity.ClassType, EntityCollection>();
        EntityRegistry.Add(StageEntity.ClassType.PLANE, new EntityCollection() { classType = StageEntity.ClassType.PLANE });
        EntityRegistry.Add(StageEntity.ClassType.CLOUD, new EntityCollection() { classType = StageEntity.ClassType.CLOUD });
        EntityRegistry.Add(StageEntity.ClassType.BLIMP, new EntityCollection() { classType = StageEntity.ClassType.BLIMP });

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
        collection.AddEntity(entity);
    }

}