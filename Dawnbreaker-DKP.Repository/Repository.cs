using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;

namespace Dawnbreaker_DKP.Repository
{
    internal class Repository<T> : IRepository<T>
        where T : DataItem
    {
        private readonly string _LiteDbStore;

        internal Repository(string storeName)
        {
            _LiteDbStore = storeName;
            RepositoryStreamManager.EnsureStream(storeName);
        }

        public List<T> GetWhere(Expression<Func<T, bool>> filter)
        {
            var stream = RepositoryStreamManager.GetStream(_LiteDbStore);

            lock (stream._STREAMLOCK)
            {
                using (var db = new LiteDatabase(stream.MemoryStream))
                {
                    return db.GetCollection<T>()
                        .Find(filter)
                        .ToList();
                }
            }
        }

        public void RemoveWhere(Expression<Func<T, bool>> filter)
        {
            var stream = RepositoryStreamManager.GetStream(_LiteDbStore);

            lock (stream._STREAMLOCK)
            {
                using (var db = new LiteDatabase(stream.MemoryStream))
                {
                    db.GetCollection<T>()
                        .Delete(filter);
                }

                stream.IsDirty = true;
            }

            RepositoryStreamManager.EnsureSyncJob();
        }

        public bool Upsert(T item)
        {
            if (item.Id == null)
            {
                item.Id = ObjectId.NewObjectId();
            }

            var stream = RepositoryStreamManager.GetStream(_LiteDbStore);

            lock (stream._STREAMLOCK)
            {
                using (var db = new LiteDatabase(stream.MemoryStream))
                {
                    db.GetCollection<T>()
                        .Upsert(item);
                }

                stream.IsDirty = true;
            }

            RepositoryStreamManager.EnsureSyncJob();
            return true;
        }
    }

    internal static class RepositoryStreamManager
    {
        private static readonly object SyncLock = new object();
        private static readonly Dictionary<string, StreamData> OpenStreams = new Dictionary<string, StreamData>();
        private static Task SyncJob;

        static RepositoryStreamManager()
        {
            EnsureSyncJob();
        }

        internal static void EnsureSyncJob()
        {
            lock (SyncLock)
            {
                if (SyncJob != null && !SyncJob.IsCompleted) return;
                SyncJob = Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(5000);
                        foreach (var openStreamKVP in OpenStreams)
                        {
                            var filePath = openStreamKVP.Key;
                            var openStream = openStreamKVP.Value;

                            lock (openStream._STREAMLOCK)
                            {
                                if (!openStream.IsDirty) continue;

                                try
                                {
                                    var bytes = openStream.MemoryStream.ToArray();
                                    File.WriteAllBytes(filePath, bytes);
                                    openStream.IsDirty = false;
                                }
                                catch (IOException ex)
                                {
                                    
                                }
                            }
                        }
                    }
                });
            }
        }

        internal static void EnsureStream(string store)
        {
            lock (SyncLock)
            {
                if (OpenStreams.ContainsKey(store)) return;

                var fileData = File.ReadAllBytes(store);
                var ms = new MemoryStream();
                ms.Write(fileData);

                var streamData = new StreamData
                {
                    MemoryStream = ms,
                    IsDirty = false,
                };
                OpenStreams.Add(store, streamData);
            }
        }

        internal static StreamData GetStream(string store)
        {
            return OpenStreams[store];
        }
    }
}
