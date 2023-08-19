using ConsoleTables;

namespace ShowTracking
{
    class Show : BaseShow
    {
        #region Public Properties

        public string Channel { get; }

        public bool Downloaded { get; set; }

        public int Id { get; }

        public string Name { get; }

        public List<Season> Seasons { get; }

        public bool Update { get; }

        public bool Watched { get; set; }

        #endregion

        #region Private Fields

        private SqlQuery SqlQuery;

        #endregion

        #region Delegates

        delegate bool Comparison(Season season);

        #endregion

        #region Constructor

        public Show(string channel, string name)
        {
            Channel = channel;
            Name = name;
        }

        public Show(string channel, string name, bool update)
        {
            Channel = channel;
            Name = name;
            Update = update;
        }

        public Show(string channel, bool downloaded, int id, string name, 
            List<Season> seasons, bool update, bool watched, SqlQuery sqlQuery) : this(channel, name, update) 
        {
            Downloaded = downloaded;
            Id = id;
            Seasons = seasons;
            Watched = watched;
            SqlQuery = sqlQuery;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Display tables of each season of the show consisting of the episodes of each season.
        /// </summary>
        public void ListShow()
        {
            Console.WriteLine($"\n{Name} : {Id}\n");

            foreach (Season season in Seasons)
            {
                Console.WriteLine($"Season: {season.Number}");

                ConsoleTable consoleTable = new ConsoleTable("EPISODE #", "TITLE", "DOWNLOADED", 
                    "WATCHED", "DATE");
                consoleTable.Options.EnableCount = false;

                foreach (Episode episode in season.Episodes)
                {
                    string downloaded = episode.Downloaded ? "     X     " : "           ";
                    string watched = episode.Watched ? "   X   " : "";

                    consoleTable.AddRow(episode.EpisodeNumber, episode.Title, downloaded, 
                        watched, episode.Released.ToString("MM/dd/yyyy"));
                }

                consoleTable.Write();
            }
        }

        /// <summary>
        /// Choose from options available to marking episodes.
        /// </summary>
        public void Mark()
        {
            string choice = ShowTrackingMain.WriteOptions("Options: Watch (1), Download (2), " +
                "List Show (3), Cancel (4)");

            switch (choice)
            {
                case "1":
                case "watch":
                    Watch();
                    break;
                case "2":
                case "download":
                    Download();
                    break;
                case "3":
                case "list show":
                    ListShow();
                    Mark();
                    break;
                case "4":
                case "cancel":
                    break;
                default:
                    Mark();
                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Displays a list of seasons available to to the current marking type.
        /// </summary>
        /// <param name="comparison"></param>
        /// <returns> List of Seasons that fit the Comparison </returns>
        private List<Season> AvailableSeasons(Comparison comparison)
        {
            List<Season> availableSeasons = new List<Season>();

            Console.WriteLine("\nAvailable seasons are: ");

            ConsoleTable consoleTable = new ConsoleTable("SEASON", "EPISODES");
            consoleTable.Options.EnableCount = false;

            foreach (Season season in Seasons)
            {
                if (!comparison(season))
                {
                    consoleTable.AddRow(season.Number, season.Episodes.Count);
                    availableSeasons.Add(season);
                }
            }

            consoleTable.Write();

            return availableSeasons;
        }

        /// <summary>
        /// Choose from options available for marking episodes as downloaded.
        /// </summary>
        private void Download()
        {
            if (Downloaded)
            {
                Console.WriteLine("\nAll Episodes Downloaded");
                return;
            }

            string choice = ShowTrackingMain.WriteOptions("Options: Season (1), Next (2), " +
                "Out Of Order (3), List Show (4), Cancel (5), Back (6)");

            switch (choice)
            {
                case "1":
                case "season":
                    SeasonDownload();
                    break;
                case "2":
                case "next":
                    NextEpisodeDownload();
                    break;
                case "3":
                case "out of order":
                    OutOfOrderNextEpisodeDownload();
                    break;
                case "4":
                case "list show":
                    ListShow();
                    Download();
                    break;
                case "5":
                case "cancel":
                    break;
                case "6":
                case "back":
                    Mark();
                    break;
                default:
                    Download();
                    break;
            }
        }

        /// <summary>
        /// Provides Downloaded value.
        /// </summary>
        /// <param name="season"></param>
        /// <returns> Downloaded value </returns>
        private bool DownloadComparison(Season season)
        {
            return season.Downloaded;
        }

        /// <summary>
        /// Displays the next epsiode asking the user if they would like to set its value or not.
        /// </summary>
        /// <param name="episode"></param>
        /// <returns> User selection </returns>
        private string EpisodeSelection(Episode episode)
        {
            Console.Write($"\n{episode.Title}   {episode.SeasonNumber}:{episode.EpisodeNumber}   (Y/N): ");

            string answer = Console.ReadLine().ToLower().Trim();
            switch (answer)
            {
                case "y":
                case "n":
                case "cancel":
                case "back":
                    return answer;
                default:
                    return EpisodeSelection(episode);
            }
        }

        /// <summary>
        /// Sets episodes as downloaded. 
        /// </summary>
        private void NextEpisodeDownload()
        {
            foreach (Season season in Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (!episode.Downloaded)
                    {
                        string answer = EpisodeSelection(episode);
                        if (answer == "back")
                        {
                            Download();
                            return;
                        }
                        else if (answer == "n" || answer == "cancel") return;
                        else
                        {
                            SqlQuery.SetDownloadEpisode(Name, episode.Title);
                            episode.SetDownload(true);
                        }
                    }
                }
                season.SetDownload(true);
            }
            SqlQuery.SetDownloadShow(Name);
            Console.WriteLine("\nShow is Complete");
        }

        /// <summary>
        /// Sets episodes as watched and downloaded.
        /// </summary>
        private void NextEpisodeWatch()
        {
            foreach (Season season in Seasons)
            {
                foreach (Episode episode in season.Episodes)
                {
                    if (!episode.Watched)
                    {
                        string answer = EpisodeSelection(episode);
                        if (answer == "back")
                        {
                            Watch();
                            return;
                        }
                        else if (answer == "n" || answer == "cancel") return;
                        else
                        {
                            SqlQuery.SetWatchEpisode(Name, episode.Title);
                            episode.SetWatch(true);
                        }
                    }
                }
                season.SetWatch(true);
            }
            SqlQuery.SetWatchShow(Name, true);
            Console.WriteLine("\nShow is Complete");
        }

        /// <summary>
        /// Sets episodes as downloaded. Episodes can be set out of release order.
        /// </summary>
        private void OutOfOrderNextEpisodeDownload()
        {
            List<Season> availableSeasons = AvailableSeasons(DownloadComparison);
            byte seasonNumber = 0;

            Console.Write("Season: ");

            string choice = Console.ReadLine().ToLower().Trim();
            
            if (choice == "back")
            {
                Download();
                return;
            }

            if (choice == "cancel") return;

            try
            {
                seasonNumber = byte.Parse(choice);
                if (availableSeasons.FirstOrDefault(i => i.Number == seasonNumber) == null)
                {
                    OutOfOrderNextEpisodeDownload();
                    return;
                }
            }
            catch
            {
                OutOfOrderNextEpisodeDownload();
                return;
            }

            for (int season = availableSeasons.FindIndex(i => i.Number == seasonNumber);
                season < availableSeasons.Count; season++)
            {
                int index = Seasons.FindIndex(i => i.Number == availableSeasons[season].Number);

                foreach (Episode episode in Seasons[index].Episodes)
                {
                    if (!episode.Downloaded)
                    {
                        string answer = EpisodeSelection(episode);
                        if (answer.ToLower() == "back")
                        {
                            Download();
                            return;
                        }
                        else if (answer == "cancel")
                        {
                            return;
                        }
                        else if (answer == "y")
                        {
                            episode.SetDownload(true);
                            SqlQuery.SetDownloadEpisode(Name, episode.Title);

                            Console.WriteLine($"\t{episode.EpisodeNumber}.{episode.Title}");
                        }
                    }
                }
                if (Seasons[index].Episodes.TrueForAll(i => i.Downloaded))
                {
                    Seasons[index].SetDownload(true);
                }
            }
            if (Seasons.TrueForAll(i => i.Downloaded))
            {
                SqlQuery.SetDownloadShow(Name);
                Console.WriteLine("\nShow is Complete");
            }
        }

        /// <summary>
        /// Sets all episodes in a season as downloaded.
        /// </summary>
        private void SeasonDownload()
        {
            List<Season> availableSeasons = AvailableSeasons(DownloadComparison);
            Console.Write("Season: ");

            string choice = Console.ReadLine().ToLower().Trim();

            if (choice == "back")
            {
                Download();
                return;
            }

            if (choice == "cancel") return;

            byte seasonNumber = 0;

            try
            {
                seasonNumber = byte.Parse(choice);
                if (availableSeasons.FirstOrDefault(i => i.Number == seasonNumber) == null)
                {
                    SeasonDownload();
                    return;
                }
            }
            catch
            {
                SeasonDownload();
                return;
            }

            SqlQuery.SetDownloadSeason(Name, seasonNumber);

            Seasons[seasonNumber - 1].SetDownload(true);

            foreach (Episode episode in Seasons[seasonNumber - 1].Episodes)
            {
                episode.SetDownload(true);

                Console.WriteLine($"\t{episode.EpisodeNumber}.{episode.Title}");
            }

            Console.WriteLine();

            foreach (Season season in Seasons)
            {
                if (!season.Downloaded)
                {
                    SeasonDownload();
                    return;
                }
            }

            SqlQuery.SetDownloadShow(Name);
            Console.WriteLine("\nShow is complete");
        }

        /// <summary>
        /// Sets all episodes in a season as watched.
        /// </summary>
        private void SeasonWatch()
        {
            List<Season> availableSeasons = AvailableSeasons(WatchComparison);
            Console.Write("Season: ");

            string choice = Console.ReadLine().ToLower().Trim();

            if (choice == "back")
            {
                Watch();
                return;
            }

            if (choice == "cancel")
            {
                return;
            }

            byte seasonNumber = 0;

            try
            {
                seasonNumber = byte.Parse(choice);
                if (availableSeasons.FirstOrDefault(i => i.Number == seasonNumber) == null)
                {
                    SeasonWatch();
                    return;
                }
            }
            catch
            {
                SeasonWatch();
                return;
            }

            SqlQuery.SetWatchSeason(Name, seasonNumber);
            Seasons[seasonNumber - 1].SetWatch(true);
            Seasons[seasonNumber - 1].SetDownload(true);

            foreach (Episode episode in Seasons[seasonNumber - 1].Episodes)
            {
                episode.SetWatch(true);
                episode.SetDownload(true);

                Console.WriteLine($"\t{episode.EpisodeNumber}.{episode.Title}");
            }

            Console.WriteLine();

            foreach (Season season in Seasons)
            {
                if (!season.Watched)
                {
                    SeasonWatch();
                    return;
                }
            }

            SqlQuery.SetWatchShow(Name, true);
            Console.WriteLine("\nShow is complete");
        }

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

        /// <summary>
        /// Choose from options available for marking episodes as watched.
        /// </summary>
        private void Watch()
        {
            if (Watched)
            {
                Console.WriteLine("\nAll Episodes Watched");
                return;
            }

            string choice = ShowTrackingMain.WriteOptions("Options: Season (1), Next (2), List Show (3), " +
                "Cancel (4), Back (5)");

            switch (choice)
            {
                case "1":
                case "season":
                    SeasonWatch();
                    break;
                case "2":
                case "next":
                    NextEpisodeWatch();
                    break;
                case "3":
                case "list show":
                    ListShow();
                    Watch();
                    break;
                case "4":
                case "cancel":
                    break;
                case "5":
                case "back":
                    Mark();
                    break;
                default:
                    Watch();
                    break;
            }
        }

        /// <summary>
        /// Provides Watched value.
        /// </summary>
        /// <param name="season"></param>
        /// <returns></returns>
        private bool WatchComparison(Season season)
        {
            return season.Watched;
        }

        #endregion
    }
}
