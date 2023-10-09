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
            foreach(var combination in source.AllCombinationsOf(n))
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
        for (int indexToIncrement = indices.Length - 1, maxValue = 0; indexToIncrement >= 0; indexToIncrement--, maxValue++) {
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
    public static List<T> RandomSelection<T>(this IEnumerable<T> source, int n) => RandomSelection(source, n, new Random());

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
}