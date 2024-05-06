using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit.Tracker.Repository.Model
{
    public class Post
    {
        public string Key
        {
            get
            {
                return $"{Author}::{Title}";
            }
        }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int UpVotes { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, Author: {Author}, UpVotes: {UpVotes}";
        }
    }
}
