using System;
using System.Collections.Generic;
using Dawnbreaker_DKP.Data.DKP;
using Dawnbreaker_DKP.Web.Models.DKP;

namespace Dawnbreaker_DKP.Web.Utilities.DKP.Interfaces
{
    public interface IClassListUtil
    {
        List<ClassListData> GetClassListData(string raidRoster, string classList);
        bool AddEntry(ClassListEntry data);
        bool MovePlayerToBottom(string raidRoster, string classType, string playerName);
        bool HandleSKEntry(Guid raidSessionId, string playerName);
    }
}
