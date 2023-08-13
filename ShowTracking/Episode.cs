using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ShowTracking
{
    class Episode : BaseShow
    {
        #region Public Properties

        public bool Downloaded { get; set; }

        [JsonProperty("episode_number")]
        public byte EpisodeNumber { get; set; }

        [JsonProperty("air_date")]
        public DateTime Released { get; set; }

        [JsonProperty("season_number")]
        public byte SeasonNumber { get; set; }

        [JsonProperty("name")]
        public string Title { get; set; }
        public bool Watched { get; set; }

        #endregion

        #region Constructor

        public Episode(DateTime date, bool downloaded, byte episodeNumber,
            byte seasonNumber, string title, bool watched)
        {
            Released = date;
            Downloaded = downloaded;
            EpisodeNumber = episodeNumber;
            Title = title;
            Watched = watched;
            SeasonNumber = seasonNumber;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set Downloaded value.
        /// </summary>
        /// <param name="download"></param>
        public void SetDownload(bool download)
        {
            Downloaded = download;
        }

        /// <summary>
        /// Set Watched value.
        /// </summary>
        /// <param name="watch"></param>
        public void SetWatch(bool watch)
        {
            Watched = watch;
        }

        #endregion
    }
}
