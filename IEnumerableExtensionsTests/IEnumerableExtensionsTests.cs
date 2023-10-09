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
}