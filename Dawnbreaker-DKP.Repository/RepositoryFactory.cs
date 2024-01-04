using System;

namespace Dawnbreaker_DKP.Repository
{
    public static class RepositoryFactory<T>
        where T : DataItem
    {
        //Overwrite to provide mock implementations for testing
        public static Func<IRepository<T>> UserDataRepository { get; set; }
        public static Func<IRepository<T>> DKPDataRepository { get; set; }
        public static Func<IRepository<T>> SystemRepository { get; set; }
        public static Func<IRepository<T>> KeystoreRepository { get; set; }

        static RepositoryFactory()
        {
            UserDataRepository = () => new Repository<T>(@"..\UserData.ldb");
            DKPDataRepository = () => new Repository<T>(@"..\DKPData.ldb");
            SystemRepository = () => new Repository<T>(@"..\System.ldb");
            KeystoreRepository = () => new Repository<T>(@"..\Keystore.ldb");
        }
    }
}
