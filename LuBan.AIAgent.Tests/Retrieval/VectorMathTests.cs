using LuBan.AIAgent.Retrieval;

namespace LuBan.AIAgent.Tests.Retrieval;

[TestClass]
public class VectorMathTests
{
    [TestMethod]
    public void Cosine_SameVector_ReturnsOne()
    {
        var v = new float[] { 1f, 2f, 3f };
        Assert.AreEqual(1.0, VectorMath.Cosine(v, v), 0.0001);
    }

    [TestMethod]
    public void Cosine_Orthogonal_ReturnsZero()
    {
        Assert.AreEqual(0.0, VectorMath.Cosine(new float[] { 1, 0 }, new float[] { 0, 1 }), 0.0001);
    }

    [TestMethod]
    public void Cosine_ZeroVector_ReturnsZero()
    {
        Assert.AreEqual(0.0, VectorMath.Cosine(new float[] { 0, 0 }, new float[] { 1, 1 }));
    }

    [TestMethod]
    public void BytesRoundTrip_PreservesValues()
    {
        var v = new float[] { 0.5f, -1.25f, 3.14f };
        var bytes = VectorMath.ToBytes(v);
        Assert.AreEqual(12, bytes.Length);
        var back = VectorMath.ToFloats(bytes);
        CollectionAssert.AreEqual(v, back);
    }
}
