using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit.Tracker.Api.Dto
{
    [Serializable]
    public class SubRedditDto
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    [Serializable]
    public class Data
    {
        [JsonProperty("children")]
        public List<Child> Children { get; set; }

        [JsonProperty("dist")]
        public int? Dist { get; set; }
    }

    [Serializable]
    public class Child
    {
        [JsonProperty("data")]
        public Post Data { get; set; }
    }

    [Serializable]
    public class Post
    {
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("selftext")]
        public string SelfText { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("upvote_ratio")]
        public double UpvoteRatio { get; set; }

        [JsonProperty("subreddit_type")]
        public string SubredditType { get; set; }

        [JsonProperty("author_fullname")]
        public string AuthorFullname { get; set; }

        [JsonProperty("num_comments")]
        public int NumComments { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("likes")]
        public bool? Likes { get; set; }

        [JsonProperty("view_count")]
        public int? ViewCount { get; set; }

        [JsonProperty("subreddit_id")]
        public string SubredditId { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("ups")]
        public int Ups { get; set; }
    }
}
