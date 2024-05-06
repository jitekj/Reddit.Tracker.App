using Reddit.Tracker.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit.Tracker.App.Manager
{
    public interface IWorkerManager
    {
        Task TrackSubReddit();
        Task<IEnumerable<string>> GetTopUsers();
        Task<IEnumerable<TopPost>> GetTopPosts();
    }
}
