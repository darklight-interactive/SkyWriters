using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

/// <summary>
/// A utility class for selecting random items based on weight.
/// Uses a cumulative weight approach to select items.
/// </summary>
public static class WeightedDataSelector
{
    private static Dictionary<IList, float> cachedTotalWeights = new Dictionary<IList, float>();

    /// <summary>
    /// Selects a random item based on weight.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <typeparam name="TResult">The type of the result item.</typeparam>
    /// <param name="items">A list of items to choose from.</param>
    /// <param name="itemSelector">A function to select the item from the list element.</param>
    /// <param name="weightSelector">A function to select the weight from the list element.</param>
    /// <param name="defaultItem">A default item to return if the selection fails.</param>
    /// <returns>A randomly selected item based on its weight.</returns>
    public static TResult SelectRandomWeightedItem<T, TResult>(IList<T> items, Func<T, TResult> itemSelector, Func<T, float> weightSelector, TResult defaultItem = default)
    {
        if (items == null || items.Count == 0)
        {
            return defaultItem;
        }

        float totalWeight;

        // Cache the total weight to avoid recalculating if the list hasn't changed
        if (!cachedTotalWeights.TryGetValue((IList)items, out totalWeight))
        {
            totalWeight = items.Sum(weightSelector);
            cachedTotalWeights[(IList)items] = totalWeight;
        }

        // Generate a random number between 0 and totalWeight
        float randomWeight = UnityEngine.Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        // Select the item based on the random weight
        foreach (var item in items)
        {
            cumulativeWeight += weightSelector(item);
            if (randomWeight <= cumulativeWeight)
            {
                return itemSelector(item);
            }
        }

        // Fallback to the default item if something goes wrong
        return defaultItem;
    }

    /// <summary>
    /// Simplified method for selecting a random item from a list of WeightedData.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="items">A list of WeightedData items to choose from.</param>
    /// <returns>A randomly selected item based on its weight.</returns>
    public static T SelectRandomWeightedItem<T>(IList<WeightedData<T>> items)
    {
        return SelectRandomWeightedItem(items, x => x.item, x => x.weight);
    }

    /// <summary>
    /// Selects multiple random items based on weight, optionally with replacement.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="items">A list of WeightedData items to choose from.</param>
    /// <param name="count">The number of items to select.</param>
    /// <param name="withReplacement">If true, items can be selected multiple times.</param>
    /// <returns>A list of randomly selected items based on their weights.</returns>
    public static List<T> SelectMultipleRandomWeightedItems<T>(IList<WeightedData<T>> items, int count, bool withReplacement = false)
    {
        List<T> selectedItems = new List<T>();
        List<WeightedData<T>> workingList = withReplacement ? new List<WeightedData<T>>(items) : items.ToList();

        for (int i = 0; i < count; i++)
        {
            if (workingList.Count == 0)
            {
                break;
            }

            var selectedItem = SelectRandomWeightedItem(workingList);

            if (!withReplacement)
            {
                workingList.Remove(workingList.First(x => x.item.Equals(selectedItem)));
            }

            selectedItems.Add(selectedItem);
        }

        return selectedItems;
    }

    /// <summary>
    /// Clear the cache for total weights. Useful if the item list changes frequently.
    /// </summary>
    public static void ClearCache()
    {
        cachedTotalWeights.Clear();
    }
}