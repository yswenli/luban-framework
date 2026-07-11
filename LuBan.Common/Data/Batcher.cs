/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*公司名称：yswenli
*命名空间：LuBan.Common.Data
*文件名： Batcher
*版本号： V1.0.0.0
*唯一标识：9cb7b7fb-0cd9-453d-8df3-92b0c5e7c486
*当前的用户域：WALLE
*创建人： WALLE
*电子邮箱：yswenli@outlook.com
*创建时间：2022/8/1 16:52:00
*描述：
*
*=================================================
*修改标记
*修改时间：2022/8/1 16:52:00
*修改人： yswenli
*版本号： V1.0.0.0
*描述：
*
*****************************************************************************/

namespace LuBan.Common.Data
{
    /// <summary>
    /// 批量处理类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Batcher<T> : IDisposable
    {
        BlockingQueue<T> _queue;

        bool _isDisposed = false;

        int _size = 1000;

        int _timeOut = 100;

        int _maxSize = 10000;


        /// <summary>
        /// 批量处理事件
        /// </summary>
        public event OnBatchedHandler<T> OnBatched;

        /// <summary>
        /// 异常事件
        /// </summary>
        public event Action<Exception> OnError;

        /// <summary>
        /// 批量处理类
        /// </summary>
        /// <param name="size">批量处理数量上限</param>
        /// <param name="timeOut">批量处理时间上限</param>
        /// <param name="maxSize">最多待处理数据量</param>
        public Batcher(int size = 100, int timeOut = 100, int maxSize = 50000)
        {
            _size = size;

            _timeOut = timeOut;

            _maxSize = maxSize;

            _queue = new BlockingQueue<T>();

            LongExecute();
        }

        /// <summary>
        /// 执行批量数据
        /// </summary>
        private void Execute()
        {
            var list = new List<T>();

            while (!_isDisposed)
            {
                if (list.Count >= _size)
                {
                    BatchRun(ref list);
                }
                else
                {
                    var t = _queue.Dequeue(_timeOut);
                    if (t != null)
                    {
                        list.Add(t);
                    }
                    else
                    {
                        BatchRun(ref list);
                    }
                }
            }

        }
        /// <summary>
        /// 批量执行
        /// </summary>
        /// <param name="list"></param>
        private void BatchRun(ref List<T> list)
        {
            try
            {
                if (list.Count > 0)
                    OnBatched?.Invoke(list);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                list.Clear();
            }

        }

        /// <summary>
        /// 长时间执行
        /// </summary>
        private void LongExecute()
        {
            TaskUtil.LongRunning(() =>
            {
                Execute();
            });
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public void Add(T t)
        {
            if (!_isDisposed)
            {
                if (_queue.Count > _maxSize)
                {
                    throw new Exception("数据量超出队列大小");
                }
                else
                {
                    _queue.Enqueue(t);
                }
            }
            else
                throw new ObjectDisposedException("添加数据失败，Batch已经释放");
        }


        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
        }
    }


    /// <summary>
    /// 批量事件委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public delegate void OnBatchedHandler<T>(List<T> data);
}
