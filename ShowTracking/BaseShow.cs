namespace ShowTracking
{
    interface BaseShow
    {
        #region Public Properties

        bool Downloaded { get; set; }

        bool Watched { get; set; }

        #endregion

        #region Public Methods

        void SetDownload(bool download);

        void SetWatch(bool watch);

        #endregion
    }
}
