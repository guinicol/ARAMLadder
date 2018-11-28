using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARAMLadder.Services
{
    public interface IRiotAPIService
    {
        bool IsResfreshingMatch();
        Task RefreshMatch(string userName, bool refreshAll = false);
    }
}
