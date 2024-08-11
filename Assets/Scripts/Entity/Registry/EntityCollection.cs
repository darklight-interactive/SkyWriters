using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

[System.Serializable]
public class EntityCollection
{
    // ----------------- Serialized Fields -------------------
    [SerializeField, ShowOnly] StageEntity.Class _entityClass;
    [SerializeField, Range(1, 32)] int _entityLimit = 8;

    // ----------------- Data -------------------
    [SerializeField, ShowOnly] List<StageEntity> _entities = new();

    // ----------------- Constructor -------------------
    public EntityCollection(StageEntity.Class entityClass, int maxLimit, Transform parent = null)
    {
        this._entityClass = entityClass;
        this._entityLimit = maxLimit;
    }

    public void AddEntity(StageEntity entity)
    {
        if (IsCollectionFull())
        {
            Debug.LogWarning($"Cannot add entity of type {_entityClass} because the collection is full");
            return;
        }

        _entities.Add(entity);
    }

    public void RemoveEntity(StageEntity entity)
    {
        _entities.Remove(entity);
    }

    public bool Contains(StageEntity entity)
    {
        return _entities.Contains(entity);
    }

    public bool IsCollectionFull()
    {
        return _entities.Count >= _entityLimit;
    }
}