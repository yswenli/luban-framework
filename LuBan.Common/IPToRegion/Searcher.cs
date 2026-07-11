/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common.IPToRegion
*文件名： Searcher
*版本号： V1.0.0.0
*唯一标识：d13f644f-0058-4283-a703-9924e2b93cf2
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/10/19 9:47:39
*描述：Searcher
*
*=====================================================================
*修改标记
*修改时间：2021/10/19 9:47:39
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：Searcher
*
*****************************************************************************/
namespace LuBan.Common.IPToRegion;

/// <summary>
/// IP查找类，用于根据IP地址查询对应的区域信息。
/// </summary>
public class Searcher
{
    /// <summary>
    /// 头部信息的长度。
    /// </summary>
    public static int HeaderInfoLength = 256;

    /// <summary>
    /// 向量索引的行数。
    /// </summary>
    public static int VectorIndexRows = 256;

    /// <summary>
    /// 向量索引的列数。
    /// </summary>
    public static int VectorIndexCols = 256;

    /// <summary>
    /// 向量索引的大小（字节数）。
    /// </summary>
    public static int VectorIndexSize = 8;

    /// <summary>
    /// 段索引的大小（字节数）。
    /// </summary>
    public static int SegmentIndexSize = 14;

    /// <summary>
    /// 文件流句柄，用于读取数据。
    /// </summary>
    private readonly Stream handle;

    /// <summary>
    /// IO操作计数器。
    /// </summary>
    private int ioCount = 0;

    /// <summary>
    /// 获取当前的IO操作计数。
    /// </summary>
    public int IOCount => ioCount;

    /// <summary>
    /// 向量索引的字节数组。
    /// </summary>
    private readonly byte[] _vectorIndex;

    /// <summary>
    /// 内容缓冲区的字节数组。
    /// </summary>
    private readonly byte[] _contentBuff;

    /// <summary>
    /// 使用文件路径创建一个新的Searcher实例。
    /// </summary>
    /// <param name="dbPath">数据库文件路径。</param>
    /// <returns>返回一个Searcher实例。</returns>
    public static Searcher NewWithFileOnly(String dbPath)
    {
        return new Searcher(dbPath, null, null);
    }

    /// <summary>
    /// 使用文件路径和向量索引创建一个新的Searcher实例。
    /// </summary>
    /// <param name="dbPath">数据库文件路径。</param>
    /// <param name="vectorIndex">向量索引的字节数组。</param>
    /// <returns>返回一个Searcher实例。</returns>
    public static Searcher NewWithVectorIndex(String dbPath, byte[] vectorIndex)
    {
        return new Searcher(dbPath, vectorIndex, null);
    }

    /// <summary>
    /// 使用内容缓冲区创建一个新的Searcher实例。
    /// </summary>
    /// <param name="cBuff">内容缓冲区的字节数组。</param>
    /// <returns>返回一个Searcher实例。</returns>
    public static Searcher NewWithBuffer(byte[] cBuff)
    {
        return new Searcher(string.Empty, null, cBuff);
    }

    /// <summary>
    /// 构造函数，用于初始化Searcher实例。
    /// </summary>
    /// <param name="dbFile">数据库文件路径。</param>
    /// <param name="vectorIndex">向量索引的字节数组。</param>
    /// <param name="cBuff">内容缓冲区的字节数组。</param>
    public Searcher(string dbFile, byte[]? vectorIndex, byte[]? cBuff)
    {
        _vectorIndex = vectorIndex ?? [];
        if (cBuff != null)
        {
            _contentBuff = cBuff;
        }
        else
        {
            handle = File.OpenRead(dbFile);
            _contentBuff = [];
        }
    }

    /// <summary>
    /// 关闭文件流句柄。
    /// </summary>
    public void Close()
    {
        if (handle != null) handle.Close();
    }

    /// <summary>
    /// 根据IP地址字符串查询对应的区域信息。
    /// </summary>
    /// <param name="ipStr">IP地址字符串。</param>
    /// <returns>返回区域信息字符串。</returns>
    public string Search(string ipStr)
    {
        var ip = checkIP(ipStr);
        return Search(ip);
    }

    /// <summary>
    /// 根据IP地址的长整型值查询对应的区域信息。
    /// </summary>
    /// <param name="ip">IP地址的长整型值。</param>
    /// <returns>返回区域信息字符串。</returns>
    public string Search(long ip)
    {
        ioCount = 0;
        int sPtr = 0, ePtr = 0;
        int il0 = (int)((ip >> 24) & 0xFF);
        int il1 = (int)((ip >> 16) & 0xFF);
        int idx = il0 * VectorIndexCols * VectorIndexSize + il1 * VectorIndexSize;
        if (_vectorIndex.Length > 0)
        {
            sPtr = GetInt(_vectorIndex, idx);
            ePtr = GetInt(_vectorIndex, idx + 4);
        }
        else if (_contentBuff.Length > HeaderInfoLength + idx + 4)
        {
            sPtr = GetInt(_contentBuff, HeaderInfoLength + idx);
            ePtr = GetInt(_contentBuff, HeaderInfoLength + idx + 4);
        }
        else
        {
            byte[] vectorBuff = new byte[VectorIndexSize];
            Read(HeaderInfoLength + idx, vectorBuff);
            sPtr = GetInt(vectorBuff, 0);
            ePtr = GetInt(vectorBuff, 4);
        }

        // 二分查找段索引块以获取区域信息
        byte[] buff = new byte[SegmentIndexSize];
        int dataLen = -1, dataPtr = -1;
        int l = 0, h = (ePtr - sPtr) / SegmentIndexSize;
        while (l <= h)
        {
            int m = (l + h) >> 1;
            int p = sPtr + m * SegmentIndexSize;

            // 读取段索引
            Read(p, buff);
            long sip = GetIntLong(buff, 0);
            if (ip < sip)
            {
                h = m - 1;
            }
            else
            {
                long eip = GetIntLong(buff, 4);
                if (ip > eip)
                {
                    l = m + 1;
                }
                else
                {
                    dataLen = GetInt2(buff, 8);
                    dataPtr = GetInt(buff, 10);
                    break;
                }
            }
        }

        // 如果未匹配到数据，返回null
        if (dataPtr < 0) return string.Empty;

        // 加载并返回区域数据
        byte[] regionBuff = new byte[dataLen];
        Read(dataPtr, regionBuff);
        return Encoding.UTF8.GetString(regionBuff);
    }

    protected virtual void Read(int offset, byte[] buffer)
    {
        if (_contentBuff != null)
        {
            Array.Copy(_contentBuff, offset, buffer, 0, buffer.Length);
            return;
        }

        // read from the file handle
        if (handle == null) throw new ArgumentNullException(nameof(handle));
        handle.Seek(offset, SeekOrigin.Begin);

        ioCount++;
        var rLen = handle.Read(buffer, 0, buffer.Length);
        if (rLen != buffer.Length) throw new IOException("incomplete read: read bytes should be " + buffer.Length);
    }

    // --- static cache util function
    public static Header LoadHeader(Stream stream)
    {
        if (stream.CanRead)
            stream.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[HeaderInfoLength];
        stream.Read(buffer, 0, HeaderInfoLength);
        return new Header(buffer);
    }
    public static Header LoadHeaderFromFile(string dbPath)
    {
        using (var fs = File.OpenRead(dbPath)) return LoadHeader(fs);
    }
    public static byte[] LoadVectorIndex(Stream stream)
    {
        if (stream.CanRead)
            stream.Seek(HeaderInfoLength, SeekOrigin.Begin);
        int len = VectorIndexRows * VectorIndexCols * SegmentIndexSize;
        var buff = new byte[len];
        var rLen = stream.Read(buff, 0, buff.Length);
        if (rLen != len) throw new IOException("incomplete read: read bytes should be " + len);
        return buff;
    }
    public static byte[] LoadVectorIndexFromFile(string dbPath)
    {
        using (var fs = File.OpenRead(dbPath)) return LoadVectorIndex(fs);
    }
    public static byte[] LoadContent(Stream stream)
    {
        if (stream.CanRead)
            stream.Seek(0, SeekOrigin.Begin);
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }

    public static byte[] LoadContentFromFile(string dbPath)
    {
        using (var fs = File.OpenRead(dbPath)) return LoadContent(fs);
    }

    public static int GetInt2(byte[] b, int offset)
    {
        return (
            (b[offset++] & 0x000000FF) |
            (b[offset] & 0x0000FF00)
        );
    }
    public static int GetInt(byte[] b, int offset)
    {
        return (
            ((b[offset++]) & 0x000000FF) |
            ((b[offset++] << 8) & 0x0000FF00) |
            ((b[offset++] << 16) & 0x00FF0000) |
            (int)((b[offset] << 24) & 0xFF000000)
        );
    }
    public static long GetIntLong(byte[] b, int offset)
    {
        return (
            ((b[offset++] & 0x000000FFL)) |
            ((b[offset++] << 8) & 0x0000FF00L) |
            ((b[offset++] << 16) & 0x00FF0000L) |
            ((b[offset] << 24) & 0xFF000000L)
        );
    }

    /* long int to ip string */
    public static string Long2ip(long ip)
    {
        return string.Join(".", (ip >> 24) & 0xFF, (ip >> 16) & 0xFF, (ip >> 8) & 0xFF, (ip) & 0xFF);
    }

    public static byte[] shiftIndex = { 24, 16, 8, 0 };

    /* check the specified ip address */
    public static long checkIP(String ip)
    {
        String[]
        ps = ip.Split('.');
        if (ps.Length != 4) throw new Exception("invalid ip address `" + ip + "`");

        long ipDst = 0;
        for (int i = 0; i < ps.Length; i++)
        {
            int val = Convert.ToInt32(ps[i]);
            if (val > 255)
            {
                throw new Exception("ip part `" + ps[i] + "` should be less than 256");
            }
            ipDst |= ((long)val << shiftIndex[i]);
        }

        return ipDst & 0xFFFFFFFFL;
    }
}
