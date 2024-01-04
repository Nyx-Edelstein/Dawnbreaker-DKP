using System;
using Dawnbreaker_DKP.Repository;

namespace Dawnbreaker_DKP.Data.Utility
{
    public class ExceptionLog : DataItem
    {
        public string Message { get; set; }
        public string ExceptionType { get; set; }

        public ExceptionLog()
        {
            Id = Guid.NewGuid();
        }
    }
}
