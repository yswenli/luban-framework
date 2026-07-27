namespace LuBan.AIAgent.Retrieval;

/// <summary>
/// 向量计算工具
/// </summary>
public static class VectorMath
{
    /// <summary>
    /// 余弦相似度
    /// </summary>
    public static double Cosine(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 0;
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        if (na == 0 || nb == 0) return 0;
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }

    /// <summary>
    /// float[] 序列化为字节
    /// </summary>
    public static byte[] ToBytes(float[] vector)
    {
        var bytes = new byte[vector.Length * 4];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// 字节反序列化为 float[]
    /// </summary>
    public static float[] ToFloats(byte[] bytes)
    {
        var vector = new float[bytes.Length / 4];
        Buffer.BlockCopy(bytes, 0, vector, 0, bytes.Length);
        return vector;
    }
}
