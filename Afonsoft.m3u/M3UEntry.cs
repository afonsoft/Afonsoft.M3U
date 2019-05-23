using System;
using System.Collections.Generic;

namespace Afonsoft.m3u
{
    /// <summary>
    /// M3UEntry
    /// </summary>
    public class M3UEntry : IComparer<M3UEntry>, IComparable<M3UEntry>
    {
        /// <summary>
        /// M3UEntry
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="title"></param>
        /// <param name="logo"></param>
        /// <param name="group"></param>
        /// <param name="name"></param>
        /// <param name="channelId"></param>
        /// <param name="epgId"></param>
        /// <param name="path"></param>
        public M3UEntry(TimeSpan duration, string title, string logo, string group, string name, string channelId,
            string epgId, Uri path)
        {
            Duration = duration;
            Title = title;
            Path = path;
            Logo = logo;
            Group = group;
            Name = name;
            ChannelId = channelId;
            EpgId = epgId;
        }

        /// <summary>
        /// Duration
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Group 
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Path 
        /// </summary>
        public Uri Path { get; set; }
        /// <summary>
        /// Logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// ChannelId
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// EpgId 
        /// </summary>
        public string EpgId { get; set; }
        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Compare
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(M3UEntry x, M3UEntry y)
        {
            if (x != null && y != null)
            {
                int r = string.CompareOrdinal(x.Group, y.Group);
                if (r == 0)
                    r = string.CompareOrdinal(x.Title, y.Title);
                if (r == 0)
                    r = string.CompareOrdinal(x.Name, y.Name);
                return r;
            }

            return 0;
        }

        /// <summary>
        /// CompareTo
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareTo(M3UEntry y)
        {
            return y != null ? Compare(this, y) : 0;
        }
    }
}
