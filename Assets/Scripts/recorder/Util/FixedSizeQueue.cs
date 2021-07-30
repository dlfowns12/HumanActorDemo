using System;
using System.Collections.Generic;
using System.Linq;

namespace DVCRecorder
{
    /// <summary>
    /// 泛型队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizedQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _size;

        public FixedSizedQueue(int size)
        {
            _size = size;
        }

        /// <summary>
        /// 塞入item
        /// </summary>
        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);

            while (_queue.Count > _size)
            {
                var o = _queue.Dequeue();
                if (o is UnityEngine.Object) UnityEngine.Object.Destroy(o as UnityEngine.Object);
            }
        }

        /// <summary>
        /// 返回索引的item
        /// </summary>
        public T ElementAt(int index)
        {
            return _queue.ElementAt(index);
        }
        
        /// <summary>
        /// 返回队列长度
        /// </summary>
        public int Count()
        {
            return _queue.Count;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            var queueId = GC.GetGeneration(_queue);
            _queue.Clear();
            GC.Collect(queueId, GCCollectionMode.Forced);
        }
    }
}