using System.IO;

namespace Dawnbreaker_DKP.Repository
{
    public class StreamData
    {
        public readonly object _STREAMLOCK = new object();
        public MemoryStream MemoryStream { get; set; }
        public bool IsDirty { get; set; }
    }
}
