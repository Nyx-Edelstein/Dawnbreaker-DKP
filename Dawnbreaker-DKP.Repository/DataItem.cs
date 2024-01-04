using LiteDB;

namespace Dawnbreaker_DKP.Repository
{
    public abstract class DataItem
    {
        public BsonValue Id { get; set; }
    }
}
