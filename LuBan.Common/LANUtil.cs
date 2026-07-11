/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：YSWENLI
*公司名称：yswenli
*命名空间：LuBan.Common
*文件名： LANUtil
*版本号： V1.0.0.0
*唯一标识：0e4ba94b-104d-4228-b9ca-c3f1dd3a1459
*当前的用户域：yswenli
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2022/6/21 14:19:53
*描述：局域网工具类
*
*=====================================================================
*修改标记
*修改时间：2022/6/21 14:19:53
*修改人： Walle.Wen
*版本号： V1.0.0.0
*描述：局域网工具类
*
*****************************************************************************/
namespace LuBan.Common
{
    /// <summary>
    /// 局域网工具类
    /// </summary>
    public static class LANUtil
    {
        /// <summary>
        /// 连接远程共享文件夹
        /// </summary>
        /// <param name="lanPath">远程共享文件夹的路径</param>
        /// <param name="userName">用户名</param>
        /// <param name="pwd">密码</param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool TryConnect(string lanPath, string userName, string pwd, out string error)
        {
            bool Flag = false;
            Process proc = new();
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.StandardInput.WriteLine("net use * /del /y");  //中断开所有连接
                string dosLine = "NET USE \"" + lanPath + "\" \"" + pwd + "\" /USER:\"" + userName + "\" /PERSISTENT:YES"; ;
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");
                while (!proc.HasExited)
                {
                    proc.WaitForExit(1000);
                }
                error = proc.StandardError.ReadToEnd();
                proc.StandardError.Close();
                if (String.IsNullOrEmpty(error))
                {
                    Flag = true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }
            return Flag;
        }


        /// <summary>
        /// 向远程文件夹保存本地内容，或者从远程文件夹下载文件到本地
        /// </summary>
        /// <param name="src">要保存的文件的路径，如果保存文件到共享文件夹，这个路径就是本地文件路径如：@"D:\1.avi"</param>
        /// <param name="dst">保存文件的路径，不含名称及扩展名</param>
        /// <param name="fileName">保存文件的名称以及扩展名</param>
        public static void Transport(string src, string dst, string fileName)
        {
            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
            }
            dst = dst + fileName;
            
            using FileStream inFileStream = new FileStream(src, FileMode.Open);
            using FileStream outFileStream = new FileStream(dst, FileMode.OpenOrCreate);

            byte[] buf = new byte[inFileStream.Length];

            int byteCount;

            while ((byteCount = inFileStream.Read(buf, 0, buf.Length)) > 0)
            {
                outFileStream.Write(buf, 0, byteCount);
            }

            inFileStream.Flush();
            outFileStream.Flush();
        }

        /// <summary>
        /// 获取局域网目录文件名
        /// </summary>
        /// <param name="lanPath"></param>
        /// <returns></returns>
        public static List<string> GetLanFileNames(string lanPath)
        {
            List<string> list = new List<string>();
            DirectoryInfo di = new DirectoryInfo(lanPath);
            StringBuilder sb = new StringPlus();
            foreach (var file in di.GetFiles())
            {
                list.Add(file.Name);
            }
            return list;
        }

        /// <summary>
        /// 获取全部文件
        /// </summary>
        /// <param name="lanPath"></param>
        /// <returns></returns>
        public static List<string> GetLanAllFileNames(string lanPath)
        {
            List<string> list = new List<string>();
            DirectoryInfo di = new DirectoryInfo(lanPath);
            StringBuilder sb = new StringPlus();
            foreach (var file in di.GetFiles())
            {
                list.Add(file.Name);
            }
            var dirs = PathUtil.GetAllDirs(lanPath);
            foreach (var sdir in dirs)
            {
                foreach (var file in sdir.GetFiles())
                {
                    list.Add(file.Name);
                }
            }
            return list;
        }


    }
}
