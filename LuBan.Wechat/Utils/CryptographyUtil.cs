/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：WALLE
*公司名称：河之洲
*命名空间：LuBan.Wechat.Utils
*文件名： Cryptography
*版本号： V1.0.0.0
*唯一标识：ec5028a4-72c0-47a1-865c-db1097ddbc26
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2024/11/14 13:16:08
*描述：微信加密解密工具类
*
*=================================================
*修改标记
*修改时间：2024/11/14 13:16:08
*修改人： yswenli
*版本号： V1.0.0.0
*描述：微信加密解密工具类
*
*****************************************************************************/


namespace LuBan.Wechat.Utils;

/// <summary>
/// 微信加密解密工具类
/// </summary>
public static class CryptographyUtil
{
    public static UInt32 HostToNetworkOrder(UInt32 inval)
    {
        UInt32 outval = 0;
        for (int i = 0; i < 4; i++)
            outval = (outval << 8) + ((inval >> (i * 8)) & 255);
        return outval;
    }

    public static Int32 HostToNetworkOrder(Int32 inval)
    {
        Int32 outval = 0;
        for (int i = 0; i < 4; i++)
            outval = (outval << 8) + ((inval >> (i * 8)) & 255);
        return outval;
    }
    /// <summary>
    /// 解密方法
    /// </summary>
    /// <param name="Input">密文</param>
    /// <param name="EncodingAESKey"></param>
    /// <returns></returns>
    /// 
    public static string AES_decrypt(String Input, string EncodingAESKey, ref string corpid)
    {
        byte[] Key;
        Key = Convert.FromBase64String(EncodingAESKey + "=");
        byte[] Iv = new byte[16];
        Array.Copy(Key, Iv, 16);
        byte[] btmpMsg = AES_decrypt(Input, Iv, Key);

        int len = BitConverter.ToInt32(btmpMsg, 16);
        len = IPAddress.NetworkToHostOrder(len);


        byte[] bMsg = new byte[len];
        byte[] bCorpid = new byte[btmpMsg.Length - 20 - len];
        Array.Copy(btmpMsg, 20, bMsg, 0, len);
        Array.Copy(btmpMsg, 20 + len, bCorpid, 0, btmpMsg.Length - 20 - len);
        string oriMsg = Encoding.UTF8.GetString(bMsg);
        corpid = Encoding.UTF8.GetString(bCorpid);


        return oriMsg;
    }

    public static String AES_encrypt(String Input, string EncodingAESKey, string corpid)
    {
        byte[] Key;
        Key = Convert.FromBase64String(EncodingAESKey + "=");
        byte[] Iv = new byte[16];
        Array.Copy(Key, Iv, 16);
        string Randcode = CreateRandCode(16);
        byte[] bRand = Encoding.UTF8.GetBytes(Randcode);
        byte[] bCorpid = Encoding.UTF8.GetBytes(corpid);
        byte[] btmpMsg = Encoding.UTF8.GetBytes(Input);
        byte[] bMsgLen = BitConverter.GetBytes(HostToNetworkOrder(btmpMsg.Length));
        byte[] bMsg = new byte[bRand.Length + bMsgLen.Length + bCorpid.Length + btmpMsg.Length];

        Array.Copy(bRand, bMsg, bRand.Length);
        Array.Copy(bMsgLen, 0, bMsg, bRand.Length, bMsgLen.Length);
        Array.Copy(btmpMsg, 0, bMsg, bRand.Length + bMsgLen.Length, btmpMsg.Length);
        Array.Copy(bCorpid, 0, bMsg, bRand.Length + bMsgLen.Length + btmpMsg.Length, bCorpid.Length);

        return AES_encrypt(bMsg, Iv, Key);

    }
    /// <summary>
    /// 生成随机码
    /// </summary>
    /// <param name="codeLen">随机码长度</param>
    /// <returns>生成的随机码字符串</returns>
    private static string CreateRandCode(int codeLen)
    {
        // 定义随机码字符集
        string codeSerial = "2,3,4,5,6,7,a,c,d,e,f,h,i,j,k,m,n,p,r,s,t,A,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,U,V,W,X,Y,Z";

        // 如果未指定长度，默认长度为16
        if (codeLen == 0)
        {
            codeLen = 16;
        }

        // 将字符集分割为数组
        string[] arr = codeSerial.Split(',');

        // 初始化随机码字符串
        string code = "";

        // 随机数生成器
        int randValue = -1;
        Random rand = new Random(unchecked((int)DateTime.Now.Ticks));

        // 循环生成随机码
        for (int i = 0; i < codeLen; i++)
        {
            // 从字符集中随机选择一个字符
            randValue = rand.Next(0, arr.Length - 1);
            code += arr[randValue];
        }

        // 返回生成的随机码
        return code;
    }

    private static String AES_encrypt(byte[] Input, byte[] Iv, byte[] Key)
    {
        var aes = new RijndaelManaged();
        //秘钥的大小，以位为单位
        aes.KeySize = 256;
        //支持的块大小
        aes.BlockSize = 128;
        //填充模式
        //aes.Padding = PaddingMode.PKCS7;
        aes.Padding = PaddingMode.None;
        aes.Mode = CipherMode.CBC;
        aes.Key = Key;
        aes.IV = Iv;
        var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[]? xBuff = null;

        #region 自己进行PKCS7补位，用系统自己带的不行
        byte[] msg = new byte[Input.Length + 32 - Input.Length % 32];
        Array.Copy(Input, msg, Input.Length);
        byte[] pad = KCS7Encoder(Input.Length);
        Array.Copy(pad, 0, msg, Input.Length, pad.Length);
        #endregion

        #region 注释的也是一种方法，效果一样
        //ICryptoTransform transform = aes.CreateEncryptor();
        //byte[] xBuff = transform.TransformFinalBlock(msg, 0, msg.Length);
        #endregion

        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
            {
                cs.Write(msg, 0, msg.Length);
            }
            xBuff = ms.ToArray();
        }

        String Output = Convert.ToBase64String(xBuff);
        return Output;
    }

    private static byte[] KCS7Encoder(int text_length)
    {
        int block_size = 32;
        // 计算需要填充的位数
        int amount_to_pad = block_size - (text_length % block_size);
        if (amount_to_pad == 0)
        {
            amount_to_pad = block_size;
        }
        // 获得补位所用的字符
        char pad_chr = chr(amount_to_pad);
        string tmp = "";
        for (int index = 0; index < amount_to_pad; index++)
        {
            tmp += pad_chr;
        }
        return Encoding.UTF8.GetBytes(tmp);
    }
    /**
     * 将数字转化成ASCII码对应的字符，用于对明文进行补码
     * 
     * @param a 需要转化的数字
     * @return 转化得到的字符
     */
    static char chr(int a)
    {

        byte target = (byte)(a & 0xFF);
        return (char)target;
    }
    private static byte[] AES_decrypt(String Input, byte[] Iv, byte[] Key)
    {
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        aes.Key = Key;
        aes.IV = Iv;
        var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[]? xBuff = null;
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Convert.FromBase64String(Input);
                byte[] msg = new byte[xXml.Length + 32 - xXml.Length % 32];
                Array.Copy(xXml, msg, xXml.Length);
                cs.Write(xXml, 0, xXml.Length);
            }
            xBuff = decode2(ms.ToArray());
        }
        return xBuff;
    }
    private static byte[] decode2(byte[] decrypted)
    {
        int pad = (int)decrypted[decrypted.Length - 1];
        if (pad < 1 || pad > 32)
        {
            pad = 0;
        }
        byte[] res = new byte[decrypted.Length - pad];
        Array.Copy(decrypted, 0, res, 0, decrypted.Length - pad);
        return res;
    }
}
