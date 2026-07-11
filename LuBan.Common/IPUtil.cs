/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： IPUtil
*版本号： V1.0.0.0
*唯一标识：97745287-62c9-4639-8266-70ae812a7229
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2021/6/16 9:43:02
*描述：ip地址工具类
*
*=====================================================================
*修改标记
*修改时间：2021/6/16 9:43:02
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：ip地址工具类
*
*****************************************************************************/


using Region = LuBan.Common.IPToRegion.Region;

namespace LuBan.Common
{
    /// <summary>
    /// ip地址工具类
    /// </summary>
    public static class IPUtil
    {
        static readonly Dictionary<int, List<string>> LocalIps = new();

        /// <summary>
        /// 获取全部网卡的本机网络地址
        /// </summary>
        /// <param name="ipv4">1:ipv4,2:ipv6,3:全部</param>
        /// <returns></returns>
        public static List<string> GetLocalIps(int ipv4 = 1)
        {
            using var lockInfo = LockerBuilder.Default.Create("IPUtil.GetOrSet");

            if (!LocalIps.TryGetValue(ipv4, out List<string>? value))
            {
                List<string> list = [];

                NetworkInterface[] NetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface NetworkIntf in NetworkInterfaces)
                {
                    IPInterfaceProperties IPInterfaceProperties = NetworkIntf.GetIPProperties();

                    if (IPInterfaceProperties.GatewayAddresses.Count <= 0) continue;

                    List<UnicastIPAddressInformation> ipAddressInfos;

                    switch (ipv4)
                    {
                        case 1:
                            ipAddressInfos = IPInterfaceProperties.UnicastAddresses.Where(b => b.Address.AddressFamily == AddressFamily.InterNetwork).ToList();
                            break;
                        case 2:
                            ipAddressInfos = IPInterfaceProperties.UnicastAddresses.Where(b => b.Address.AddressFamily == AddressFamily.InterNetworkV6).ToList();
                            break;
                        default:
                            ipAddressInfos = IPInterfaceProperties.UnicastAddresses.Where(b => b.Address.AddressFamily == AddressFamily.InterNetwork || b.Address.AddressFamily == AddressFamily.InterNetworkV6).ToList();
                            break;
                    }

                    if (ipAddressInfos != null && ipAddressInfos.Any()) list.AddRange(ipAddressInfos.Select(b => b.Address.ToString()));
                }

                value = list;
                LocalIps[ipv4] = value;
            }
            return value;
        }

        /// <summary>
        /// 获取本地网络地址
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ipv4">1:ipv4,2:ipv6,3:全部</param>
        /// <returns></returns>
        public static string GetLocalIp(int index = 0, int ipv4 = 1)
        {
            var list = GetLocalIps(ipv4);

            if (list == null || !list.Any() || index >= list.Count) return string.Empty;

            return list[index];
        }

        /// <summary>
        /// 获取第一个可用的端口号
        /// </summary>
        /// <returns></returns>
        public static int GetFirstAvailablePort()
        {
            int BEGIN_PORT = 1024;//从这个端口开始检测
            int MAX_PORT = 65535; //系统tcp/udp端口数最大是65535

            for (int i = BEGIN_PORT; i < MAX_PORT; i++)
            {
                if (PortIsAvailable(i)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 检查指定端口是否已用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsAvailable(int port)
        {
            bool isAvailable = true;

            IList portUsed = PortIsUsed();

            foreach (int p in portUsed)
            {
                if (p == port)
                {
                    isAvailable = false; break;
                }
            }

            return isAvailable;
        }


        /// <summary>
        /// 获取操作系统已用的端口号
        /// </summary>
        /// <returns></returns>
        private static IList PortIsUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序
            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序
            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            IList allPorts = new ArrayList();
            foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
            foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
            foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);

            return allPorts;
        }


        /// <summary>
        /// ip转成long
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static long IpToInt(string ip)
        {
            char[] separator = new char[] { '.' };
            string[] items = ip.Split(separator);
            return long.Parse(items[0]) << 24
                    | long.Parse(items[1]) << 16
                    | long.Parse(items[2]) << 8
                    | long.Parse(items[3]);
        }

        /// <summary>
        /// long转成ip
        /// </summary>
        /// <param name="ipInt"></param>
        /// <returns></returns>
        public static string IntToIp(long ipInt)
        {
            StringBuilder sb = new StringPlus();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }

        #region 获取外网ip
        /// <summary>
        /// 获取外网ip
        /// </summary>
        /// <returns></returns>
        public async static Task<Tuple<string, string>?> GetInternetIP()
        {
            try
            {
                var httpClient = HttpClientProxy.Create("http://oa.oceania-sh.com/");
                var result = await httpClient.GetViewModelAsync<IPResponse>("Api/Ip/index");
                if (result != null && result.Msg != null)
                {
                    return new Tuple<string, string>(result.Msg.TrueIp, result.Msg.WebIp);
                }
            }
            catch { }
            return null;
        }
        [DataContract]
        internal class IPResponse
        {
            [DataMember(Name = "code")]
            public int Code { get; set; }

            [DataMember(Name = "msg")]
            public Msg2 Msg { get; set; }
        }
        [DataContract]
        internal class Msg2
        {

            [DataMember(Name = "true_ip")]
            public string TrueIp { get; set; }

            [DataMember(Name = "web_ip")]
            public string WebIp { get; set; }
        }
        #endregion

        #region 根据ip获取地区

        /// <summary>
        /// 根据ip获取地区
        /// </summary>
        /// <param name="ipStr"></param>
        /// <returns></returns>
        public static string IpToRegion(this string ipStr)
        {
            if (string.IsNullOrWhiteSpace(ipStr)) return string.Empty;
            if (!System.Net.IPAddress.TryParse(ipStr, out var addr) || addr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return string.Empty;
            try { return Region.Get(ipStr); }
            catch { return string.Empty; }
        }

        #endregion
    }
}
