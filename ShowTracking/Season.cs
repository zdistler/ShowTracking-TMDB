namespace ShowTracking
{
    class Season : APIResponse, BaseShow
    {
        #region Public Properties

        public bool Downloaded { get; set; }
        public List<Episode> Episodes { get; set; }
        public byte Number { get; set; }
        public bool Watched { get; set; }

        #endregion

        #region Constructor

        public Season(bool downloaded, List<Episode> episodes, byte number, bool watched)
        {
            Downloaded = downloaded;
            Episodes = episodes;
            Number = number;
            Watched = watched;
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
        /// Set Number value.
        /// </summary>
        /// <param name="number"></param>
        public void SetNumber(byte number)
        {
            Number = number;
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
