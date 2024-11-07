namespace IEnumerableExtensions;

public static class IEnumerableExtensions {
    /// <summary>
    /// Returns a sequence of sequences, each of which is a chunk of the given size from the source sequence.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="chunkSize">maximum size of each returned sequence</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>a stream of streams</returns>
    /// <exception cref="ArgumentNullException">if source is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">if chunkSize less than 1</exception>
    /// <remarks>
    /// Does NOT turn any part of the source into a list so is O(1) memory efficient for any source
    /// stream size and any chunk size.
    /// </remarks>
    public static IEnumerable<IEnumerable<T>> StreamChunks<T>(this IEnumerable<T> source, int chunkSize) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize), chunkSize, "must be > 0");
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
            yield return StreamChunksInner(enumerator, chunkSize);
    }

    private static IEnumerable<T> StreamChunksInner<T>(IEnumerator<T> enumerator, int chunkSize) {
        do
            yield return enumerator.Current;
        while (--chunkSize > 0 && enumerator.MoveNext());
    }

    /// <summary>
    /// Returns all combinations of n items from the source sequence.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="n">number of items for each combination</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>all combinations of n items</returns>
    /// <exception cref="ArgumentNullException">if source is null</exception>
    /// <remarks>returns empty if n less than zero or n is greater than source length</remarks>
    public static IEnumerable<IEnumerable<T>> AllCombinationsOf<T>(this IEnumerable<T> source, int n) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (n > 0) {
            var list = source.ToList();
            var size = list.Count;
            if (n <= size) {
                var indices = Enumerable.Range(0, n).ToArray();
                for (;;) {
                    yield return indices.Select(i => list[i]);
                    int indexToIncrement = n - 1;
                    while (indexToIncrement >= 0 && indices[indexToIncrement] == size - n + indexToIncrement)
                        indexToIncrement--;
                    if (indexToIncrement < 0) break;
                    indices[indexToIncrement]++;
                    for (int indexToReset = indexToIncrement + 1; indexToReset < n; indexToReset++)
                        indices[indexToReset] = indices[indexToReset - 1] + 1;
                }
            }
        }
    }

    /// <summary>
    /// Returns all permutations of n items from the source sequence.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="n">number of items for each permutation</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>all permutations of n items</returns>
    /// <exception cref="ArgumentNullException">if source is null</exception>
    /// <remarks>returns empty if n less than zero or n is greater than source length</remarks>
    public static IEnumerable<IEnumerable<T>> AllPermutationsOf<T>(this IEnumerable<T> source, int n) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (n > 0)
            foreach (var combination in source.AllCombinationsOf(n))
                foreach (var permutation in combination.AllPermutationsOf())
                    yield return permutation;
    }

    /// <summary>
    /// Returns all permutations of items from the source sequence.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>all permutations of items</returns>
    /// <exception cref="ArgumentNullException">if source is null</exception>
    public static IEnumerable<IEnumerable<T>> AllPermutationsOf<T>(this IEnumerable<T> source) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var list = source.ToList();
        var size = list.Count;
        if (size > 0) {
            int[] indices = new int[size];
            do {
                var copy = list.ToList();
                // Note: .ToList() required below to force enumeration of indices to return permutation
                yield return indices.Select(i => copy.RemoveAtIndex(i)).ToList();
            } while (PermutationIncrementIndices(indices));
        }
    }

    private static bool PermutationIncrementIndices(int[] indices) {
        for (int indexToIncrement = indices.Length - 1, maxValue = 0;
             indexToIncrement >= 0;
             indexToIncrement--, maxValue++) {
            if (indices[indexToIncrement]++ < maxValue)
                return true;
            indices[indexToIncrement] = 0;
        }
        return false;
    }

    private static T RemoveAtIndex<T>(this IList<T> list, int index) {
        var result = list[index];
        list.RemoveAt(index);
        return result;
    }

    /// <summary>
    /// Gets a random selection of n items from the source sequence.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="n">number of items to get</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>list of n items chosen at random in a random order</returns>
    /// <remarks>Only iterates the source once -- does not turn source into a list</remarks>
    public static List<T> RandomSelection<T>(this IEnumerable<T> source, int n) =>
        RandomSelection(source, n, new Random());

    /// <summary>
    /// Gets a random selection of n items from the source sequence.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="n">number of items to get</param>
    /// <param name="random">random number generator to use for selection</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>list of n items chosen at random in a random order</returns>
    /// <remarks>Only iterates the source once -- does not turn source into a list</remarks>
    public static List<T> RandomSelection<T>(this IEnumerable<T> source, int n, Random random) {
        int c = 0;
        var list = new List<T>(n);
        foreach (var item in source) {
            if (c++ < n)
                list.Add(item);
            else if (random.Next(c) < n)
                list[random.Next(n)] = item;
        }
        return list;
    }

    /// <summary>
    /// Performs an action on each item in the source sequence. Note that the source sequence is
    /// explicitly and completely enumerated before any of the source sequence is returned.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="action">action to perform on each item</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source sequence</returns>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (var item in source)
            action(item);
        return source;
    }

    /// <summary>
    /// Performs an action on each item in the source collection.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="action">action to perform on each item</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source collection</returns>
    public static ICollection<T> Do<T>(this ICollection<T> source, Action<T> action) {
        foreach (var item in source)
            action(item);
        return source;
    }

    /// <summary>
    /// Performs an action on each item in the source list.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="action">action to perform on each item</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source list</returns>
    public static IList<T> Do<T>(this IList<T> source, Action<T> action) {
        foreach (var item in source)
            action(item);
        return source;
    }

    /// <summary>
    /// Performs an action on each item in the source list.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="action">action to perform on each item</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source list</returns>
    public static List<T> Do<T>(this List<T> source, Action<T> action) {
        foreach (var item in source)
            action(item);
        return source;
    }

    /// <summary>
    /// Performs an action on each item in the source array.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="action">action to perform on each item</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source array</returns>
    public static T[] Do<T>(this T[] source, Action<T> action) {
        foreach (var item in source)
            action(item);
        return source;
    }

    /// <summary>
    /// Returns a slice of the source sequence, start at the given index and ending at the given index.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="start">starting item index to return</param>
    /// <param name="end">ending item index (exclusive)</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source sequence from start to, but not including, end</returns>
    /// <remarks>
    /// Does not turn source into a list. Allocates minimal memory possible.
    /// Iterates the source sequence only once and only as far as necessary.
    /// Only delays the returned enumeration as much as necessary.
    /// </remarks>
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, Index start, Index end) =>
        source.Slice(start..end);

    /// <summary>
    /// Returns a slice of the source sequence within the given range.
    /// </summary>
    /// <param name="source">source sequence</param>
    /// <param name="range">range of item indexes to return</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source sequence from start to, but not including, end</returns>
    /// <remarks>
    /// Does not turn source into a list. Allocates minimal memory possible.
    /// Iterates the source sequence only once and only as far as necessary.
    /// Only delays the returned enumeration as much as necessary.
    /// </remarks>
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, Range range) {
        if (range.Start.IsFromEnd) {
            Queue<T> queue = new(range.Start.Value + 1); // +1 because we enqueue before dequeue
            int count = 0;
            foreach (T item in source) {
                queue.Enqueue(item);
                if (queue.Count > range.Start.Value) {
                    queue.Dequeue();
                }
                count++;
            }

            if (range.End.IsFromEnd) {
                return queue.Take(range.Start.Value - range.End.Value);
            } else {
                return queue.Take(range.Start.Value + range.End.Value - count);
            }
        } else {
            source = source.Skip(range.Start.Value);
            if (range.End.IsFromEnd) {
                return range.End.Value == 0 ? source : SkipLast(source, range.End.Value);
            } else {
                return source.Take(range.End.Value - range.Start.Value);
            }
        }

        static IEnumerable<T> SkipLast(IEnumerable<T> source, int skipLast) {
            Queue<T> queue = new(skipLast + 1);
            foreach (T item in source) {
                queue.Enqueue(item);
                if (queue.Count > skipLast) {
                    yield return queue.Dequeue();
                }
            }
        }
    }

    /// <summary>
    /// Returns a slice of the source collection, starting at the given index and ending at the given index.
    /// </summary>
    /// <param name="source">source collection</param>
    /// <param name="start">starting item index to return</param>
    /// <param name="end">ending item index (exclusive)</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source collection from start to, but not including, end</returns>
    public static ICollection<T> Slice<T>(this ICollection<T> source, Index start, Index end) =>
        source.Slice(start..end);

    /// <summary>
    /// Returns a slice of the source collection within the given range.
    /// </summary>
    /// <param name="source">source collection</param>
    /// <param name="range">range of item indexes to return</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source collection from start to, but not including, end</returns>
    public static ICollection<T> Slice<T>(this ICollection<T> source, Range range) {
        (int offset, int length) = range.GetOffsetAndLength(source.Count);
        return source.Skip(offset).Take(length).ToList();
    }

    /// <summary>
    /// Returns a slice of the source list, starting at the given index and ending at the given index.
    /// </summary>
    /// <param name="source">source list</param>
    /// <param name="start">starting item index to return</param>
    /// <param name="end">ending item index (exclusive)</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source list from start to, but not including, end</returns>
    public static IList<T> Slice<T>(this IList<T> source, Index start, Index end) =>
        source.Slice(start..end);

    /// <summary>
    /// Returns a slice of the source list within the given range.
    /// </summary>
    /// <param name="source">source list</param>
    /// <param name="range">range of item indexes to return</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source list from start to, but not including, end</returns>
    public static IList<T> Slice<T>(this IList<T> source, Range range) {
        (int offset, int length) = range.GetOffsetAndLength(source.Count);
        return source.Skip(offset).Take(length).ToList();
    }

    /// <summary>
    /// Returns a slice of the source list, starting at the given index and ending at the given index.
    /// </summary>
    /// <param name="source">source list</param>
    /// <param name="start">starting item index to return</param>
    /// <param name="end">ending item index (exclusive)</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source list from start to, but not including, end</returns>
    public static List<T> Slice<T>(this List<T> source, Index start, Index end) =>
        source.Slice(start..end);

    /// <summary>
    /// Returns a slice of the source list within the given range.
    /// </summary>
    /// <param name="source">source list</param>
    /// <param name="range">range of item indexes to return</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source list from start to, but not including, end</returns>
    public static List<T> Slice<T>(this List<T> source, Range range) {
        (int offset, int length) = range.GetOffsetAndLength(source.Count);
        return source.GetRange(offset, length);
    }

    /// <summary>
    /// Returns a slice of the source array, starting at the given index and ending at the given index.
    /// </summary>
    /// <param name="source">source array</param>
    /// <param name="start">starting item index to return</param>
    /// <param name="end">ending item index (exclusive)</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source array from start to, but not including, end</returns>
    public static T[] Slice<T>(this T[] source, Index start, Index end) =>
        source.Slice(start..end);

    /// <summary>
    /// Returns a slice of the source array within the given range.
    /// </summary>
    /// <param name="source">source array</param>
    /// <param name="range">range of item indexes to return</param>
    /// <typeparam name="T">type of item</typeparam>
    /// <returns>source array from start to, but not including, end</returns>
    public static T[] Slice<T>(this T[] source, Range range) => source[range];
}