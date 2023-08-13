namespace ShowTracking
{
    class TvSeriesInfoAPI
    {
        #region Public Properties

        public string[] Seasons { private get; set; }
        public int SeasonTotal => Seasons.Length;

        #endregion
    }
}
