using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    /// <summary>
    /// A useful data structure.
    /// Similar to a Queue or Stack, but every time an item is pushed onto it,
    /// the IPQ puts the item in the correctly-sorted place.
    /// </summary>
public class IndexedPriorityQueue<T>
{
    private bool ascending;

    private List<T> items;
    private List<float> costs;

    public bool IsEmpty
    {
        get { return items.Count == 0; }
    }

    /// <summary>
    /// Creates a new IPQ.
    /// </summary>
    /// <param name="ascending">If true, this IPQ will return the "cheapest" items first. Otherwise, it will return the most "expensive" items first.</param>
    /// <param name="items">The list of items, or "null" if it should start empty.</param>
    /// <param name="costs">The list of items, or "null" if it should start empty. Should be the same size as "items".</param>
    /// <remarks>"items" should be parallel to "costs", but the lists don't have to be SORTED according to "costs" when passed in. They are automatically sorted.</remarks>
    public IndexedPriorityQueue(bool ascending, List<T> items, List<int> costs)
    {
        if (items == null)
        {
            items = new List<T>();
            costs = new List<int>();
        }

        if (items.Count != costs.Count)
            throw new ArgumentOutOfRangeException("The list of items and the list of costs are not the same size!");

        this.items = new List<T>();
        this.costs = new List<float>();

        this.ascending = ascending;

        //Sort the lists.
        for (int i = 0; i < items.Count; ++i)
            Push(items[i], costs[i]);
    }

    /// <summary>
    /// If the given cost exists in the queue, finds the smallest index for it. Otherwise, finds the index it should be inserted at.
    /// </summary>
    /// <remarks>The index will be equal to costs.Count if the given cost is greater than any other cost.</remarks>
    private int GetIndex(float cost)
    {
        //Outlier checking.
        if (costs.Count == 0) return 0;
        if (costs[0] >= cost) return 0;
        if (costs[costs.Count - 1] < cost) return costs.Count;

        //Use binary search.
        Interval searchInterval = new Interval(0, costs.Count, 0);

        //Narrow the search range down using binary search.
        while (searchInterval.Range > 2)
            if (costs[(int)searchInterval.Center] > cost)
                searchInterval = searchInterval.FirstHalf;
            else if (costs[(int)searchInterval.Center] < cost)
                searchInterval = searchInterval.SecondHalf;
            else searchInterval = new Interval(0, searchInterval.Center, 0);

        //Now find the furthest-back index with the given cost.
        int ret = (int)(searchInterval.End - searchInterval.RoundedEpsilon);
        while (ret > 0 && costs[ret - 1] >= cost)
            --ret;
        return ret;
    }

    /// <summary>
    /// Adds a new element to the IPQ.
    /// </summary>
    public void Push(T item, float cost)
    {
        //Get the position the item belongs at.
        int index = GetIndex(cost);
        //If it belongs at the end, add it to the end.
        if (index == costs.Count)
        {
            costs.Add(cost);
            items.Add(item);
        }
        //Otherwise, we have to insert it.
        else
        {
            costs.Insert(index, cost);
            items.Insert(index, item);
        }
    }
    /// <summary>
    /// Removes the given item from the IPQ.
    /// </summary>
    public void Remove(T item)
    {
        Remove(items.IndexOf(item));
    }
    private void Remove(int index)
    {
        items.RemoveAt(index);
        costs.RemoveAt(index);
    }

    public void Clear()
    {
        items.Clear();
        costs.Clear();
    }

    /// <summary>
    /// Gets the top element off the IPQ, as well as its cost.
    /// </summary>
    public KeyValuePair<float, T> Pop()
    {
        int i = ascending ? 0 : (items.Count - 1);
        float cost = costs[i];
        T ret = items[i];

        items.RemoveAt(i);
        costs.RemoveAt(i);

        return new KeyValuePair<float, T>(cost, ret);
    }

    /// <summary>
    /// Gets the cost of the given item.
    /// </summary>
    public float GetCost(T item)
    {
        return GetCost(items.IndexOf(item));
    }
    private float GetCost(int index)
    {
        return costs[index];
    }

    /// <summary>
    /// Changes the cost of the given item.
    /// </summary>
    public void UpdateCost(T item, float newCost)
    {
        //Iterate through the list and 1) remove the old item position, and 2) add the new item position.
        bool addedNew = false;
        bool removedOld = false;
        for (int i = 0; i < items.Count; ++i)
        {
            if (!removedOld && items[i].Equals(item))
            {
                items.RemoveAt(i);
                costs.RemoveAt(i);
                removedOld = true;
            }
            if (!addedNew && costs[i] > newCost)
            {
                costs.Insert(i, newCost);
                items.Insert(i, item);
                addedNew = true;
            }
        }

        return;
    }

    /// <summary>
    /// Swaps two items in this IPQ.
    /// </summary>
    /// <param name="pos1">The position of the first item.</param>
    /// <param name="pos2">The position of the second item.</param>
    private void Swap(int pos1, int pos2)
    {
        T old = items[pos1];
        float oldC = costs[pos1];

        items[pos1] = items[pos2];
        costs[pos1] = costs[pos2];
        items[pos2] = old;
        costs[pos2] = oldC;
    }

    public override string ToString()
    {
        string s = "An indexed priority queue with the following elements in " +
                    (ascending ? "ascending" : "descending") +
                    " order:";

        if (ascending)
            for (int i = 0; i < items.Count; ++i)
                s += "\n" + items[i].ToString() + " with cost " + costs[i].ToString();
        else for (int i = items.Count - 1; i >= 0; --i)
                s += "\n" + items[i].ToString() + " with cost " + costs[i].ToString();

        return s;
    }
}
