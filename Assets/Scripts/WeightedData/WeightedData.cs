


using UnityEngine;

/// <summary>
/// A generic class to store an item and its weight.
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class WeightedData<T>
{
    public T item;
    [Range(0f, 1f)] public float weight;

    public WeightedData(T item, float weight)
    {
        this.item = item;
        this.weight = weight;
    }
}

