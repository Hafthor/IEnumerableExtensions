using System.Collections.Immutable;
using IEnumerableExtensions;

namespace IEnumerableExtensionsTests;

[TestClass]
public class IEnumerableExtensionsTests {
    [TestMethod]
    public void TestStreamChunks() {
        var input = Enumerable.Range(0, 50);
        var chunks = input.StreamChunks(10);
        int expected = 0;
        int chunkNumber = 0;
        foreach (var chunk in chunks) {
            foreach (var v in chunk)
                Assert.AreEqual(expected++, v);
            chunkNumber++;
            Assert.AreEqual(chunkNumber * 10, expected);
        }
        Assert.AreEqual(5, chunkNumber);
    }

    [TestMethod]
    public void TestAllCombinationsOf() {
        var deck = Enumerable.Range(0, 52);
        var hands = deck.AllCombinationsOf(5);
        var combinations = 52 * 51 * 50 * 49 * 48 / 5 / 4 / 3 / 2 / 1;
        Assert.AreEqual(combinations, hands.Count());
        
        Assert.AreEqual("0,1,2,3,4", string.Join(',', hands.First()));
        Assert.AreEqual("47,48,49,50,51", string.Join(',', hands.Last()));

        Assert.ThrowsException<ArgumentNullException>(() => ((IEnumerable<int>)null).AllCombinationsOf(0).Any());

        Assert.AreEqual(1, deck.AllCombinationsOf(52).Count());
        Assert.AreEqual(0, deck.AllCombinationsOf(53).Count());
        Assert.AreEqual(0, deck.AllCombinationsOf(-1).Count());
        Assert.AreEqual(0, deck.AllCombinationsOf(0).Count());
    }

    [TestMethod]
    public void TestAllPermutationsOf() {
        var deck = Enumerable.Range(0, 5);
        var permutations = deck.AllPermutationsOf(2);
        Assert.AreEqual(20, permutations.Count());
        Assert.AreEqual("0,1", string.Join(',', permutations.First()));
        Assert.AreEqual("4,3", string.Join(',', permutations.Last()));
        
        Assert.ThrowsException<ArgumentNullException>(() => ((IEnumerable<int>)null).AllPermutationsOf(0).Any());
        
        Assert.AreEqual(5 * 4 * 3 * 2 * 1, deck.AllPermutationsOf(5).Count());
        Assert.AreEqual(0, deck.AllPermutationsOf(0).Count());
        Assert.AreEqual(0, deck.AllPermutationsOf(6).Count());
        Assert.AreEqual(0, deck.AllPermutationsOf(-1).Count());
    
        var deck2 = new List<string> { "a", "b", "c" };
        var permutations2 = deck2.AllPermutationsOf();
        Assert.AreEqual(6, permutations2.Count());
        Assert.AreEqual("a,b,c", string.Join(',', permutations2.First()));
        Assert.AreEqual("c,b,a", string.Join(',', permutations2.Last()));
    }

    [TestMethod]
    public void TestRandomSelection() {
        var deck = Enumerable.Range(0, 52);
        var hand = deck.RandomSelection(5, new Random(0));
        Assert.AreEqual("28,39,2,23,16", string.Join(",", hand));

        var statistics = new int[52];
        int samples = 1000000;
        for(int r = 0; r < samples; r++) {
            hand = deck.RandomSelection(5, new Random(r));
            foreach (var card in hand)
                statistics[card]++;
        }
        double avg = statistics.Average();
        double stDev = Math.Sqrt(statistics.Sum(x => (x - avg) * (x - avg)) / samples);
        Assert.IsTrue(stDev < 2.0);
    }

    [TestMethod]
    public void TestSlice() {
        string s = "Hello world";

        Range r = 3..^3;
        Assert.AreEqual("lo wo", s.Slice(r));
        CollectionAssert.AreEqual("lo wo".ToCharArray(), s.ToCharArray().Slice(r).ToArray());
        CollectionAssert.AreEqual("lo wo".ToImmutableList(), s.ToImmutableList().Slice(r).ToImmutableList());

        r = ^5..6;
        Assert.AreEqual("", s.Slice(r));
        CollectionAssert.AreEqual(Array.Empty<char>(), s.ToCharArray().Slice(r).ToArray());
        CollectionAssert.AreEqual(Array.Empty<char>(), s.ToImmutableList().Slice(r).ToImmutableList());
        
        // Test slice on IEnumerable
        {
            CollectionAssert.AreEqual(Emit().Slice(1..3).ToArray(), "el".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(..3).ToArray(), "Hel".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(1..^2).ToArray(), "el".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(1..).ToArray(), "ello".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(^4..3).ToArray(), "el".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(^4..).ToArray(), "ello".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(^4..^2).ToArray(), "el".ToCharArray());
            CollectionAssert.AreEqual(Emit().Slice(..^2).ToArray(), "Hel".ToCharArray());

            static IEnumerable<char> Emit() => "Hello".Select(c => c);
        }
        
        // Testing slice streaming
        {
            Assert.AreEqual("HEeLl", LoggedEmitSliceLowerAndConsume(1..3)); // no lag, doesn't consume last 2
            Assert.AreEqual("HhEeLl", LoggedEmitSliceLowerAndConsume(..3)); // no lag, doesn't consume last 2
            Assert.AreEqual("HELLeOl", LoggedEmitSliceLowerAndConsume(1..^2)); // lag of 2
            Assert.AreEqual("HEeLlLlOo", LoggedEmitSliceLowerAndConsume(1..)); // no lag
            Assert.AreEqual("HELLOel", LoggedEmitSliceLowerAndConsume(^4..3)); // read to end
            Assert.AreEqual("HELLOello", LoggedEmitSliceLowerAndConsume(^4..)); // read to end
            Assert.AreEqual("HELLOel", LoggedEmitSliceLowerAndConsume(^4..^2)); // read to end
            Assert.AreEqual("HELhLeOl", LoggedEmitSliceLowerAndConsume(..^2)); // lag of 2

            static string LoggedEmitSliceLowerAndConsume(Range range) {
                string actionLog = "";
                LoggedEmit(LoggedEmit("HELLO").Slice(range).Select(char.ToLower)).All(_ => true);
                return actionLog;

                IEnumerable<char> LoggedEmit(IEnumerable<char> source) {
                    return source.Select(c => {
                        actionLog += c;
                        return c;
                    });
                }
            }
        }
    }
}