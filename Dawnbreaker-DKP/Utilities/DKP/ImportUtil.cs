using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Data.DKP.Enum;
using Dawnbreaker_DKP.Repository;
using Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces;

namespace Dawnbreaker_DKP.Web.Utilities.DKP
{
    public class ImportUtil : IImportUtil
    {
        private IRepository<PlayerRecord> PlayerRecordRepository { get; set; }

        public ImportUtil(IRepository<PlayerRecord> playerRecordRepository)
        {
            PlayerRecordRepository = playerRecordRepository;
        }

        public bool TryImport(string data)
        {
            data = data.Replace("\r", "").Replace("\n", "");
            data = Regex.Replace(data, "\\s*\\<[a-zA-z0-9 \"=:;]*\\>\\s*", "");
            data = Regex.Replace(data, "\\<\\/div\\>\\<[a-zA-Z =\":\\/\\.]*_", ", ");
            data = Regex.Replace(data, "\\.jpg\"[ a-z=\"0-9]*\\>\\<\\/div\\>", ", ");
            data = Regex.Replace(data, "\\<\\/div\\>\\s*\\<\\/div\\>", "; ");

            var playerRecords = data.Split("; ", StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    var playerData = x.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    var playerName = playerData[0];
                    Enum.TryParse<Class>(playerData[1], true, out var playerClass);
                    var playerDKP = int.Parse(playerData[2]);

                    return new PlayerRecord
                    {
                        PlayerName = playerName,
                        Class = playerClass,
                        Spec = string.Empty,
                        DKPCurrent = playerDKP
                    };
                }).ToList();

            foreach (var record in playerRecords)
            {
                PlayerRecordRepository.Upsert(record);
            }

            return true;
        }
    }
}
