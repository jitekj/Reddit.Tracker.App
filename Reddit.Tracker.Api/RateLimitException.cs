using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit.Tracker.Api
{
    /// <summary>
    /// Exception to indicate Rate limit was exceeded
    /// </summary>
    public class RateLimitException : Exception
    {
        public RateLimitException():base("Rate Limit Exceeded") { }
    }
}
