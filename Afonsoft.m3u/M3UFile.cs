using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Afonsoft.m3u.Extensions;

namespace Afonsoft.m3u
{ 
    /// <summary>
    /// M3UFile
    /// </summary>
    public class M3UFile : ICollection<M3UEntry>, IComparer<M3UFile>, IComparable<M3UFile>
    {
        private readonly List<M3UEntry> _entries = new List<M3UEntry>();

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Site 
        /// </summary>
        public string Site { get; set; }
        /// <summary>
        /// Phone
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Author 
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// Cover 
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// Description 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Logo 
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// Compare
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(M3UFile x, M3UFile y)
        {
            if (x != null && y != null)
            {
                int r = string.CompareOrdinal(x.Name, y.Name);
                if (r == 0)
                    r = string.CompareOrdinal(x.Author, y.Author);
                if (r == 0)
                    r = string.CompareOrdinal(x.Email, y.Email);
                return r;
            }

            return 0;
        }

        /// <summary>
        /// CompareTo
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public int CompareTo(M3UFile y)
        {
            return y != null ? Compare(this, y) : 0;
        }

        /// <summary>
        /// Array M3UEntry
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public M3UEntry this[int index] => _entries[index];

        /// <summary>
        /// M3UFile
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="resolveRelativePaths"></param>
        public M3UFile(string fileName, bool resolveRelativePaths = false)
        {
            Load(fileName, resolveRelativePaths);
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="resolveRelativePaths"></param>
        public void Load(string fileName, bool resolveRelativePaths = false)
        {
            _entries.Clear();
            if (string.IsNullOrEmpty(fileName))
                throw new M3UException("fime is missing.");

            using (var reader = new StreamReader(fileName))
            {
                var workingUri = new Uri(Path.GetDirectoryName(fileName) ?? throw new M3UException("fime is missing."));

                string line;
                var lineCount = 0;

                M3UEntry entry = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (lineCount == 0 && !line.StartsWith("#EXTM3U"))
                        throw new M3UException($"Line {lineCount}: M3U header is missing.");

                    //Recuperar as informações desta lista
                    if (line.StartsWith("#PLAYLISTV:"))
                    {
                        //Remove "#PLAYLISTV:"
                        var newLine = line.Substring(11, line.Length - 11);

                        Email = GetValue(newLine, "pltv-email");
                        Site = GetValue(newLine, "pltv-site");
                        Phone = GetValue(newLine, "pltv-phone");
                        Author = GetValue(newLine, "pltv-author");
                        Cover = GetValue(newLine, "pltv-cover");
                        Description = GetValue(newLine, "pltv-description");
                        Name = GetValue(newLine, "pltv-name");
                        Logo = GetValue(newLine, "pltv-logo");
                    }

                    //Recuperar a lista
                    if (line.StartsWith("#EXTINF:"))
                    {
                        if (entry != null)
                            throw new M3UException($"Line {lineCount}: Unexpected entry detected.");

                        //Remove "#EXTINF:"
                        var split = line.Substring(8, line.Length - 8).Split(new[] { ',' }, 2);

                        if (split.Length != 2)
                            throw new M3UException($"Line {lineCount}: Invalid track information.");

                        var newLine = split[0].Trim();
                        var time = newLine.IndexOf(' ') != -1 ? newLine.Substring(0, newLine.IndexOf(' ')).Trim() : newLine.Trim();

                        if (!int.TryParse(time, out int seconds))
                            throw new M3UException($"Line {lineCount}: Invalid track duration.");

                        var title = split[1].Replace(",", "").Trim();
                        var logo = GetValue(newLine, "tvg-logo").Trim();
                        var group = GetValue(newLine, "group-title").Trim();
                        var channelId = GetValue(newLine, "channel-id").Trim();
                        var epgId = GetValue(newLine, "epg-id").Trim();
                        var name = GetValue(newLine, "tvg-name").Trim();

                        var duration = TimeSpan.FromSeconds(seconds);

                        if (string.IsNullOrEmpty(name))
                            name = title;

                        entry = new M3UEntry(duration, title, logo, group, name, channelId, epgId, null);
                    }

                    else if (entry != null && !line.StartsWith("#")) //ignore comments
                    {
                        if (!Uri.TryCreate(line, UriKind.RelativeOrAbsolute, out var path))
                            throw new M3UException($"Line {lineCount}: Invalid entry path.");

                        if (path.IsFile && resolveRelativePaths)
                            path = path.MakeAbsoluteUri(workingUri);

                        entry.Path = path;

                        _entries.Add(entry);

                        entry = null;
                    }

                    lineCount++;
                }
            }
        }

        private static string GetValue(string line, string value, string comma = "\"")
        {
            if (line.IndexOf(value, StringComparison.Ordinal) != -1)
            {
                try
                {
                    string stringAfterChar = line.Substring(line.IndexOf(value, StringComparison.Ordinal) + value.Length + 1);
                    int firstStringPosition = stringAfterChar.IndexOf(comma, StringComparison.Ordinal) + 1;
                    if (firstStringPosition > 1) firstStringPosition = 0;
                    int LastStringPosition = stringAfterChar.Substring(firstStringPosition).IndexOf(comma, StringComparison.Ordinal);
                    if (LastStringPosition == -1) LastStringPosition = firstStringPosition;
                    return stringAfterChar.Substring(firstStringPosition, LastStringPosition).Trim();
                }
                catch (Exception ex)
                {
                    throw new M3UException($"Invalid value '{value}' to get in M3U File", ex);
                }
            }

            return "";
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="useAbsolutePaths"></param>
        /// <param name="useLocalFilePath"></param>
        public void Save(string fileName, bool useAbsolutePaths = false, bool useLocalFilePath = true)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new M3UException("file is missing.");

            var workingUri = new Uri(Path.GetDirectoryName(fileName) ?? throw new M3UException("fime is missing."));

            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine("#EXTM3U");

                if (!string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Author))
                    writer.WriteLine("#PLAYLISTV: pltv-logo=\"{0}\" pltv-name=\"{1}\" pltv-description=\"{2}\" pltv-cover=\"{3}\" pltv-author=\"{4}\" pltv-site=\"{5}\" pltv-email=\"{6}\" pltv-phone=\"{7}\"", Logo, Name, Description, Cover, Author, Site, Email, Phone);

                foreach (var entry in this)
                {
                    writer.WriteLine("#EXTINF:{0} channel-id=\"{1}\" epg-id=\"{2}\" tvg-name=\"{3}\" group-title=\"{4}\" tvg-logo=\"{5}\",{6}", entry.Duration.TotalSeconds, entry.ChannelId, entry.EpgId, entry.Name, entry.Group, entry.Logo, entry.Title);

                    if (entry.Path.IsFile && useLocalFilePath)
                        writer.WriteLine(entry.Path.LocalPath);
                    else if (!entry.Path.IsAbsoluteUri && useAbsolutePaths)
                        writer.WriteLine(entry.Path.MakeAbsoluteUri(workingUri));
                    else
                        writer.WriteLine(entry.Path);
                }
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<M3UEntry> GetEnumerator()
        {
            return ((IEnumerable<M3UEntry>)_entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<M3UEntry>

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="item"></param>
        public void Add(M3UEntry item)
        {
            _entries.Add(item);
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(M3UEntry item)
        {
            return _entries.Contains(item);
        }

        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(M3UEntry[] array, int arrayIndex)
        {
            _entries.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(M3UEntry item)
        {
            return _entries.Remove(item);
        }

        /// <summary>
        /// Count 
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// IsReadOnly 
        /// </summary>
        public bool IsReadOnly => false;

        #endregion

        /// <summary>
        /// Find
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public M3UEntry Find(Predicate<M3UEntry> match)
        {
            return _entries.Find(match);
        }

        /// <summary>
        /// FindAll
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<M3UEntry> FindAll(Predicate<M3UEntry> match)
        {
            return _entries.FindAll(match);
        }
    }
}
