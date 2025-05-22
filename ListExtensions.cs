using System;
using System.Collections.Generic;

static class ListExtensions
{
    /// <summary>
    /// Selects a random item of a List.
    /// Usage => my item = myList.RandomItem();
    /// </summary>
    public static T RandomElement<T>(this List<T> list)
    {
        var rnd = new Random();
        var randomIndex = rnd.Next(list.Count);
        return list[randomIndex];
    }

    /// <summary>
    /// Randomizes the ordering of a List.
    /// Usage => mylist.Shuffle();
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        var rnd = new Random();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var randomIndex = rnd.Next(i + 1); //maxValue (i + 1) is EXCLUSIVE
            list.Swap(i, randomIndex);
        }
    }

    // public static void Shuffle<T>(this IList<T> ts) {
	// 	var count = ts.Count;
	// 	var last = count - 1;
    //     var rnd = new Random();
	// 	for (var i = 0; i < last; ++i)
    //     {
    //         var r = rnd.Next(i, count);
    //         // var r = UnityEngine.Random.Range(i, count);
    //         var tmp = ts[i];
    //         ts[i] = ts[r];
    //         ts[r] = tmp;
    //     }
	// }

    /// <summary>
    /// Swaps two elements in a List.
    /// Usage => myList.Swap(4, 2);
    /// </summary>
    public static void Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        (list[indexB], list[indexA]) = (list[indexA], list[indexB]);
    }

    /// <summary>
    /// Removes an element from the List and returns it. The removed element is replaced
    /// by the last element of the List. This does not preserve ordering, but is O(1).
    /// Usage => var element = myList.SwapRemove(4);
    /// </summary>
    public static T SwapRemove<T>(this IList<T> list, int index)
    {
        var element = list[index];
        var indexLast = list.Count - 1;
        list[index] = list[indexLast];
        list.RemoveAt(indexLast);
        return element;
    }

    /// <summary>
    /// Removes and returns the last item in a List.
    /// Usage => var element = myList.Pop();
    /// </summary>
    public static T Pop<T>(this List<T> list)
    {
        var index = list.Count - 1;
        var element = list[index];
        list.RemoveAt(index);
        return element;
    }

    /// <summary>
    /// Removes and returns the item in the List at the given index.
    /// Usage => var element = myList.RemoveAndReturn(42);
    /// </summary>
    public static T RemoveAndReturnAt<T>(this List<T> list, int index)
    {
        var element = list[index];
        list.RemoveAt(index);
        return element;
    }

    public static void WriteLine<T>(this List<T> list)
    {
        foreach (var item in list)
        {
            if (item != null)
            {
                System.Console.Write(item.ToString() + ", ");
            }
        }
        System.Console.WriteLine("");
    }
}
