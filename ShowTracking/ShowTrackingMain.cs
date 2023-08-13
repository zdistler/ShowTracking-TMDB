using ConsoleTables;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShowTracking
{
    class ShowTrackingMain
    {
        #region Private Fields

        private SqlQuery SqlQuery;
        private APIRequest APIRequest;

        #endregion

        #region Delegates

        private delegate void ChooseShowDelegate(List<Show> shows);

        #endregion

        #region Constructor

        private ShowTrackingMain()
        {
            using (StreamReader streamReader = new StreamReader("Database.json"))
            {
                string database = streamReader.ReadToEnd();
                SqlQuery = JsonConvert.DeserializeObject<SqlQuery>(database);
            }

            using (StreamReader streamReader = new StreamReader("TMDB.json"))
            {
                string bearer = streamReader.ReadToEnd();
                APIRequest = JsonConvert.DeserializeObject<APIRequest>(bearer);
            }
        }

        #endregion

        #region Main

        static void Main(string[] args)
        {
            ShowTrackingMain showTrackingMain = new ShowTrackingMain();
            
            showTrackingMain.Options();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Display options and returns the user choice
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string WriteOptions(string options)
        {
            Console.WriteLine($"\n{options}");
            Console.Write("Choice: ");
            return Console.ReadLine().ToLower().Trim();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Finds all seasons and returns them in a list.
        /// </summary>
        /// <param name="seasonTotal"></param>
        /// <param name="id"></param>
        /// <returns> List of Seasons </returns>
        private async Task<List<Season>?> AddSeasons(int seasonTotal, int id)
        {
            List<Season> seasons = new List<Season>();

            for (int seasonNumber = 1; seasonNumber <= seasonTotal; seasonNumber++)
            {
                Season? nextSeason = await APIRequest.GetSeason(id, seasonNumber);
                
                if (nextSeason == null || nextSeason.ErrorCheck()) break;

                nextSeason.Episodes.RemoveAll(i => i.Released > DateTime.Now);

                if (nextSeason.Episodes.Count == 0) break;

                seasons.Add(nextSeason);
            }

            return seasons;
        }

        /// <summary>
        /// Creates the show consisting of all currently available seasons and episodes.
        /// </summary>
        /// <returns></returns>
        private async Task AddShow()
        {
            Console.Write("Enter Show Name (` to cancel): ");
            string name = Console.ReadLine().ToLower().Trim();

            if (name == "`") return;

            name = name.Replace('/', ' ').Replace('\\', ' ');

            SearchSeriesAPI searchSeries = await APIRequest.GetId(name);

            if (searchSeries.ErrorCheck()) return;

            SeriesResultAPI? seriesResult = null;

            if (searchSeries.Results.Count > 1)
            {
                seriesResult = SelectSearchSeries(searchSeries);
            }
            else if (searchSeries.Results.Count == 1)
            {
                seriesResult = searchSeries.Results[0];
            }

            if (seriesResult == null) return;

            seriesResult = await APIRequest.GetShow(seriesResult.Id);

            if (SqlQuery.GetShows().Find(show => show.Id == seriesResult.Id) != null)
            {
                Console.WriteLine("Show has already been added");
                return;
            }

            string channel = seriesResult.ChooseNetwork();
            
            if (channel == "cancel") return;

            if (channel == "none")
            {
                channel = "";
            }

            List<Season> seasons = await AddSeasons(seriesResult.SeasonsTotal, seriesResult.Id);

            Show show = default(Show);

            if (SqlQuery.GetShows().Find(show => show.Name == seriesResult.Name) != null)
            {
                show = new Show(channel, false, seriesResult.Id, $"{seriesResult.Name} ({seriesResult.Year})", seasons, false, false, SqlQuery);
            }
            else
            {
                show = new Show(channel, false, seriesResult.Id, seriesResult.Name, seasons, false, false, SqlQuery);
            }

            SqlQuery.CreateShow(show);

            show.ListShow();
        }

        /// <summary>
        /// Displays the name and description of a show for the user to select whether this is the show 
        /// they were looking for.
        /// </summary>
        /// <param name="seriesResult"></param>
        /// <returns> Show Name </returns>
        private string ChooseShow(SeriesResultAPI seriesResult)
        {
            Console.WriteLine($"\n{seriesResult.Name} ({seriesResult.Year}): {seriesResult.Overview}");
            Console.Write("Is this correct (Y/N): ");
            string answer = Console.ReadLine().ToLower().Trim();
            switch (answer)
            {
                case "y":
                case "n":
                case "cancel":
                    return answer;
                default:
                    return ChooseShow(seriesResult);
            }
        }

        /// <summary>
        /// Searches for and adds new episodes of the latest season known to this app.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="current"></param>
        /// <param name="update"></param>
        private void CheckForNewEpisodes(string title, Season current, Season update)
        {
            Console.WriteLine($"\tSeason: {update.Number}");

            if (current.Episodes.Count < update.Episodes.Count)
            {
                update.Episodes.RemoveAll(updateEpisode => updateEpisode.Released > DateTime.Now ||
                    current.Episodes.Exists(currentEpisode => currentEpisode.Title == updateEpisode.Title));

                ConsoleTable consoleTable = new ConsoleTable("NUMBER", "TITLE");
                consoleTable.Options.EnableCount = false;

                foreach (Episode episode in update.Episodes)
                {
                    consoleTable.AddRow(episode.EpisodeNumber, episode.Title);
                }

                if (consoleTable.Rows.Count > 0)
                {
                    SqlQuery.AddSeason(title, update);

                    consoleTable.Write();

                    SqlQuery.SetWatchShow(title, false);
                }
                else
                {
                    Console.WriteLine("\tNo episodes added");
                }
            }
        }

        /// <summary>
        /// Searches for and adds new seasons for the show.
        /// </summary>
        /// <param name="show"></param>
        /// <param name="seasonNumber"></param>
        private async void CheckForNewSeason(Show show, int seasonNumber)
        {
            Season nextSeason = await APIRequest.GetSeason(show.Id, seasonNumber);

            if (nextSeason == null || nextSeason.ErrorCheck()) return;

            nextSeason.Episodes.RemoveAll(episode => episode.Released > DateTime.Now);

            Console.WriteLine($"\tSeason: {seasonNumber}");

            if (nextSeason.Episodes.Count > 0)
            {
                ConsoleTable consoleTable = new ConsoleTable("NUMBER", "TITLE");
                consoleTable.Options.EnableCount = false;

                foreach (Episode episode in nextSeason.Episodes)
                {
                    consoleTable.AddRow(episode.EpisodeNumber, episode.Title);
                }

                if (consoleTable.Rows.Count > 0)
                {
                    consoleTable.Write();

                    SqlQuery.AddSeason(show.Name, nextSeason);
                    SqlQuery.SetSeasonTotal(show.Name, seasonNumber);
                    SqlQuery.SetWatchShow(show.Name, false);
                }
                else
                {
                    Console.WriteLine("\tNo episodes added");
                }
            }
            else
            {
                Console.WriteLine("\tNo episodes added");
            }
        }

        /// <summary>
        /// Displays a table of shows with the name, channel, and an associated number.
        /// </summary>
        /// <param name="shows"></param>
        private void ListShows(List<Show> shows)
        {
            ConsoleTable consoleTable = new ConsoleTable("NUMBER", "NAME", "CHANNEL");
            consoleTable.Options.EnableCount = false;

            foreach (Show show in shows)
            {
                consoleTable.AddRow(shows.IndexOf(show) + 1, show.Name, show.Channel);
            }
            consoleTable.Write();
        }

        /// <summary>
        /// Displays a table of shows with name, channel, updatable status, and an associated number.
        /// </summary>
        /// <param name="shows"></param>
        private void ListShowsUpdatable(List<Show> shows)
        {
            ConsoleTable consoleTable = new ConsoleTable("NUMBER", "NAME", "UPDATE", "CHANNEL");
            consoleTable.Options.EnableCount = false;

            foreach (Show show in shows)
            {
                string updatable = show.Update ? "   X   " : "       ";
                consoleTable.AddRow(shows.IndexOf(show) + 1, show.Name,updatable, show.Channel);
            }
            consoleTable.Write();
        }

        /// <summary>
        /// Displays a table of updatable shows with name and channel.
        /// </summary>
        private void ListUpdateShows()
        {
            List<Show> shows = SqlQuery.GetUpdateShows();
            ConsoleTable consoleTable = new ConsoleTable("NAME", "CHANNEL");

            foreach (Show show in shows)
            {
                consoleTable.AddRow(show.Name, show.Channel);
            }

            consoleTable.Write();
            Console.WriteLine();
        }

        /// <summary>
        /// Updates all shows from a user selected starting point. Updates all shows regardless of updatable status.
        /// Shows are updated in alphabetical order. If API call limit is reached the next show to be updated
        /// will be displayed to the user.
        /// </summary>
        /// <param name="title"></param>
        private async void MassUpdate(string title)
        {
            List<Show> shows = SqlQuery.GetShows();

            shows.RemoveRange(0, shows.IndexOf(shows.First(i => i.Name == title)));

            foreach (Show show in shows)
            {                
                UpdateShow(show);
            }
        }

        /// <summary>
        /// User selects from the operations supported.
        /// </summary>
        private async void Options()
        {
            while (true)
            {
                Show? show = default(Show);
                string choice = WriteOptions("Options: Add (1), Remove (2), List Updatable (3), " +
                    "Mark (4), List Show (5), updatable (6), update (7), update latest (8), " +
                    "single update (9), mass update (10), stop (0)");

                switch (choice)
                {
                    case "1":
                    case "add":
                        await AddShow();
                        break;

                    case "2":
                    case "remove":
                        show = ShowSelect(SqlQuery.GetShowNames(), ListShows);
                        if (show != null)
                        {
                            SqlQuery.RemoveShow(show.Name);
                            Console.WriteLine($"Removed {show.Name}");
                        }
                        break;

                    case "3":
                    case "list updatable":
                        ListUpdateShows();
                        break;

                    case "4":
                    case "mark":
                        show = ShowSelect(SqlQuery.GetShowNames(), ListShows);
                        if (show != null) SqlQuery.GetShow(show.Name).Mark();
                        break;

                    case "5":
                    case "list show":
                        show = ShowSelect(SqlQuery.GetShowNames(), ListShows);
                        if (show != null) SqlQuery.GetShow(show.Name).ListShow();
                        break;

                    case "6":
                    case "updatable":
                        while (true)
                        {
                            show = ShowSelect(SqlQuery.GetShowNames(), ListShowsUpdatable);
                            if (show != null) SqlQuery.SetUpdatableShow(show);
                            else break;
                        }
                        break;

                    case "7":
                    case "update":
                        UpdateShows();
                        break;

                    case "8":
                    case "update latest":
                        show = ShowSelect(SqlQuery.GetShowNames(), ListShows);
                        if (show != null) UpdateLatestSeason(SqlQuery.GetShow(show.Name));
                        break;

                    case "9":
                    case "single update":
                        show = ShowSelect(SqlQuery.GetShowNames(), ListShows);
                        if (show != null) UpdateShow(SqlQuery.GetShow(show.Name));
                        break;

                    case "10":
                    case "mass update":
                        show = ShowSelect(SqlQuery.GetShowNames(), ListShows);
                        if (show != null) MassUpdate(show.Name);
                        break;

                    case "0":
                    case "stop":
                        return;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// User selects whether the correct show to add is presented or not.
        /// </summary>
        /// <param name="searchSeries"></param>
        /// <returns> SeriesResult user selected </returns>
        private SeriesResultAPI? SelectSearchSeries(SearchSeriesAPI searchSeries)
        {
            foreach (SeriesResultAPI seriesResult in searchSeries.Results)
            {
                string answer = ChooseShow(seriesResult);
                switch (answer)
                {
                    case "y":
                        return seriesResult;
                    case "n":
                        break;
                    case "cancel":
                        return null;
                }
            }

            Console.WriteLine("Show was not found");

            return null;
        }

        /// <summary>
        /// Gets the channel associated with the show from the user.
        /// </summary>
        /// <returns> Channel Name </returns>
        private string SetChannel()
        {
            Console.Write("Channel Name: ");
            return Console.ReadLine().Trim().ToUpper();
        }

        /// <summary>
        /// Choose a show based on typing a numeric value associated with it or by name.
        /// Hitting enter shows currently available shows. 
        /// Entering text that does not match a show will narrow the list down.
        /// </summary>
        /// <param name="shows"></param>
        /// <param name="del"></param>
        /// <returns> User Selected Show </returns>
        private Show? ShowSelect(List<Show> shows, ChooseShowDelegate del)
        {
            Console.WriteLine("\nPress Enter to see list, 'cancel' to leave");
            Console.Write("Choice: ");
            string choice = Console.ReadLine().Trim();

            if (choice == "")
            {
                del(shows);
                return ShowSelect(shows, del);
            }

            try
            {
                int numericChoice = int.Parse(choice) - 1;
                if (numericChoice < shows.Count)
                {
                    return shows.ElementAt(numericChoice);
                }
                else
                {
                    Console.WriteLine("Number is out of range");
                    return ShowSelect(shows, del);
                }
            }
            catch
            {
                choice = choice.ToLower();
                if (choice == "cancel")
                {
                    return null;
                }

                Show? exactMatch = shows.Find(show => show.Name.ToLower().Equals(choice));
                if (exactMatch != null)
                {
                    return exactMatch;
                }

                List<Show> containsShows = shows.FindAll(show => show.Name.ToLower().Contains(choice));
                if (containsShows.Count == 1)
                {
                    return containsShows[0];
                }
                else if (containsShows.Count > 1)
                {
                    Console.WriteLine($"Reduced options based on '{choice}'");
                    del(containsShows);
                    return ShowSelect(containsShows, del);
                }
                else
                {
                    Console.WriteLine("There is no show containing that text");
                    return ShowSelect(shows, del);
                }
            }
        }

        /// <summary>
        /// Searches for new episodes of the show in the latest known season and for new seasons.
        /// </summary>
        /// <param name="show"></param>
        private async void UpdateShow(Show show)
        {
            Console.WriteLine($"\n{show.Name}");

            if (show.Seasons.Count > 0)
            {
                Season updateSeason = await APIRequest.GetSeason(show.Id, show.Seasons.Count);

                if (updateSeason.ErrorCheck()) return;

                Season currentSeason = show.Seasons.Last();

                CheckForNewEpisodes(show.Name, currentSeason, updateSeason);
            }

            SeriesResultAPI seriesResult = await APIRequest.GetShow(show.Id);

            int seasonTotal = seriesResult.SeasonsTotal;

            for (int seasonNumber = show.Seasons.Count + 1; seasonNumber <= seasonTotal; seasonNumber++)
            {
                CheckForNewSeason(show, seasonNumber);
            }
        }

        /// <summary>
        /// Updates all shows with a true updatable value.
        /// </summary>
        private void UpdateShows()
        {
            List<Show> shows = SqlQuery.GetUpdateShows();

            foreach (Show show in shows)
            {
                UpdateShow(show);
            }
        }

        /// <summary>
        /// Removes and recreates the latest season with downloaded and watched values saved. New episodes are also added.
        /// This exists because IMDB will make mistakes with numbering and release dates occasionally.
        /// Unsure if this is still needed.
        /// </summary>
        /// <param name="show"></param>
        private async void UpdateLatestSeason(Show show)
        {
            Season updateSeason = await APIRequest.GetSeason(show.Id, show.Seasons.Count);

            if (updateSeason.ErrorCheck()) return;

            Season lastSeason = show.Seasons.Last();

            SqlQuery.RemoveSeason(show.Name, lastSeason.Number);

            updateSeason.Episodes.RemoveAll(episode => episode.Released > DateTime.Now);

            foreach (Episode episode in updateSeason.Episodes)
            {
                Episode existingEpisode = lastSeason.Episodes.Find(i => i.Title == episode.Title);

                if (existingEpisode != null)
                {
                    episode.SetDownload(existingEpisode.Downloaded);
                    episode.SetWatch(existingEpisode.Watched);
                }
            }

            SqlQuery.AddSeason(show.Name, updateSeason);
        }

        #endregion
    }
}
