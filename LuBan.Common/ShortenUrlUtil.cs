/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.Common
*文件名： ShortenUrlUtil
*版本号： V1.0.0.0
*唯一标识：20dd46a5-3d64-4760-8055-808f0efe73b3
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2023/8/31 10:56:47
*描述：短链接生成工具
*
*=================================================
*修改标记
*修改时间：2023/8/31 10:56:47
*修改人： yswenli
*版本号： V1.0.0.0
*描述：短链接生成工具
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// 短链接生成工具
    /// </summary>
    public static class ShortenUrlUtil
    {
        const string Seq = "s9LFkgy5RovixI1aOf8UhdY3r4DMplQZJXPqebE0WSjBn7wVzmN2Gc6THCAKut";

        private static string DataFile
        {
            get { return PathUtil.GetRootFullName("cache", "Url.db"); }
        }

        private static string IndexFile
        {
            get { return PathUtil.GetRootFullName("cache", "Url.idx"); }
        }

        /// <summary>
        /// 批量添加网址，按顺序返回Key。
        /// 如果输入的一组网址中有不合法的元素，则返回数组的相同位置（下标）的元素将为null。
        /// </summary>
        /// <param name="url"></param>   
        /// <param name="exceptHost"></param>   
        /// <returns></returns>
        public static string[] AddUrl(string[] url, string exceptHost = "")
        {
            using FileStream index = new FileStream(IndexFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using FileStream data = new FileStream(DataFile, FileMode.Append, FileAccess.Write);
            data.Position = data.Length;
            DateTime now = DateTime.Now;
            byte[] dt = BitConverter.GetBytes(now.ToBinary());
            int hitCount = 0;
            byte[] hits = BitConverter.GetBytes(hitCount);
            string[] resultKey = new string[url.Length];
            int seekSeek = unchecked((int)now.Ticks);
            Random seekRand = new Random(seekSeek);
            byte[] status = BitConverter.GetBytes(true);
            //index: ID(8) + Begin(8) + Length(2) + Hits(4) + DateTime(8) = 30
            for (int i = 0; i < url.Length && i < 1000; i++)
            {
                if (url[i].Length == 0 || url[i].Length > 4096) continue;
                if (exceptHost.IsNotNullOrEmpty() && url[i].ToLower().Contains(exceptHost)) continue;
                long Begin = data.Position;
                byte[] UrlData = Encoding.UTF8.GetBytes(url[i]);
                data.Write(UrlData, 0, UrlData.Length);
                byte[] buff = new byte[8];
                long Last;
                if (index.Length >= 30) //读取上一条记录的ID
                {
                    index.Position = index.Length - 30;
                    index.Read(buff, 0, 8);
                    index.Position += 22;
                    Last = BitConverter.ToInt64(buff, 0);
                }
                else
                {
                    Last = 1000 * 1000 * 1000; //起步ID，如果太小，生成的短网址会太短。
                    index.Position = 0;
                }
                long RandKey = Last + (long)GetRnd(seekRand);
                byte[] BeginData = BitConverter.GetBytes(Begin);
                byte[] LengthData = BitConverter.GetBytes((Int16)(UrlData.Length));
                byte[] RandKeyData = BitConverter.GetBytes(RandKey);

                index.Write(RandKeyData, 0, 8);
                index.Write(BeginData, 0, 8);
                index.Write(LengthData, 0, 2);
                index.Write(hits, 0, hits.Length);
                index.Write(dt, 0, dt.Length);
                resultKey[i] = Mixup(RandKey);
            }
            return resultKey;
        }

        /// <summary>
        /// 按顺序批量解析Key，返回一组长网址。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string[] ParseUrl(string[] key)
        {
            using FileStream index = new FileStream(IndexFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using FileStream data = new FileStream(DataFile, FileMode.Open, FileAccess.Read);
            byte[] buff = new byte[8];
            long[] ids = key.Select(n => UnMixup(n)).ToArray();
            string[] result = new string[ids.Length];
            long rightDefault = (long)(index.Length / 30) - 1;
            for (int j = 0; j < ids.Length; j++)
            {
                long id = ids[j];
                long left = 0;
                long right = rightDefault;
                long middle = -1;
                while (left <= right)
                {
                    middle = (long)(Math.Floor((double)((right + left) / 2)));
                    if (middle < 0) break;
                    index.Position = middle * 30;
                    index.Read(buff, 0, 8);
                    long val = BitConverter.ToInt64(buff, 0);
                    if (val == id) break;
                    if (val < id)
                    {
                        left = middle + 1;
                    }
                    else
                    {
                        right = middle - 1;
                    }
                }
                string url = string.Empty;
                if (middle != -1)
                {
                    index.Position = middle * 30 + 8; //跳过ID          
                    index.Read(buff, 0, buff.Length);
                    long Begin = BitConverter.ToInt64(buff, 0);
                    index.Read(buff, 0, buff.Length);
                    Int16 Length = BitConverter.ToInt16(buff, 0);
                    byte[] UrlTxt = new byte[Length];
                    data.Position = Begin;
                    data.Read(UrlTxt, 0, UrlTxt.Length);
                    int Hits = BitConverter.ToInt32(buff, 2);//跳过2字节的Length
                    byte[] NewHits = BitConverter.GetBytes(Hits + 1);//解析次数递增, 4字节
                    index.Position -= 6;//指针撤回到Length之后
                    index.Write(NewHits, 0, NewHits.Length);//覆盖老的Hits
                    url = Encoding.UTF8.GetString(UrlTxt);
                }
                result[j] = url;
            }
            return result;
        }

        /// <summary>
        /// 混淆id为字符串
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string Mixup(long id)
        {
            string Key = Convert(id);
            int s = 0;
            foreach (char c in Key)
            {
                s += (int)c;
            }
            int Len = Key.Length;
            int x = (s % Len);
            char[] arr = Key.ToCharArray();
            char[] newarr = new char[arr.Length];
            System.Array.Copy(arr, x, newarr, 0, Len - x);
            System.Array.Copy(arr, 0, newarr, Len - x, x);
            string NewKey = "";
            foreach (char c in newarr)
            {
                NewKey += c;
            }
            return NewKey;
        }

        /// <summary>
        /// 解开混淆字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static long UnMixup(string key)
        {
            int s = 0;
            foreach (char c in key)
            {
                s += (int)c;
            }
            int Len = key.Length;
            int x = (s % Len);
            x = Len - x;
            char[] arr = key.ToCharArray();
            char[] newarr = new char[arr.Length];
            System.Array.Copy(arr, x, newarr, 0, Len - x);
            System.Array.Copy(arr, 0, newarr, Len - x, x);
            string NewKey = "";
            foreach (char c in newarr)
            {
                NewKey += c;
            }
            return Convert(NewKey);
        }

        /// <summary>
        /// 10进制转换为62进制
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string Convert(long id)
        {
            if (id < 62)
            {
                return Seq[(int)id].ToString();
            }
            int y = (int)(id % 62);
            long x = (long)(id / 62);

            return Convert(x) + Seq[y];
        }

        /// <summary>
        /// 将62进制转为10进制
        /// </summary>
        /// <param name="Num"></param>
        /// <returns></returns>
        private static long Convert(string Num)
        {
            long v = 0;
            int Len = Num.Length;
            for (int i = Len - 1; i >= 0; i--)
            {
                int t = Seq.IndexOf(Num[i]);
                double s = (Len - i) - 1;
                long m = (long)(Math.Pow(62, s) * t);
                v += m;
            }
            return v;
        }

        /// <summary>
        /// 生成随机的0-9a-zA-Z字符串
        /// </summary>
        /// <returns></returns>
        public static string GenerateKeys()
        {
            string[] Chars = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z".Split(',');
            int SeekSeek = unchecked((int)DateTime.Now.Ticks);
            Random SeekRand = new Random(SeekSeek);
            for (int i = 0; i < 100000; i++)
            {
                int r = SeekRand.Next(1, Chars.Length);
                string f = Chars[0];
                Chars[0] = Chars[r - 1];
                Chars[r - 1] = f;
            }
            return string.Join("", Chars);
        }

        /// <summary>
        /// 返回随机递增步长
        /// </summary>
        /// <param name="SeekRand"></param>
        /// <returns></returns>
        private static Int16 GetRnd(Random SeekRand)
        {
            return (Int16)SeekRand.Next(1, 11);
        }
    }
}
