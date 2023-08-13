using System.Data.SqlClient;
using System.Data;

namespace ShowTracking
{
    class SqlQuery
    {
        #region Private Fields

        private readonly SqlConnection SqlConnection;

        #endregion

        #region Constructor

        public SqlQuery(string dataSource, string catalog, string userId, string password)
        {
            SqlConnection = new SqlConnection(@$"Data Source={dataSource};
                Initial Catalog={catalog};User ID={userId};Password={password}");
            SqlConnection.Open();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds each episode of a season to the show's table.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="season"></param>
        public void AddSeason(string title, Season season)
        {
            foreach (Episode episode in season.Episodes)
            {
                AddEpisode(title, episode);
            }
        }

        /// <summary>
        /// Adds the show to the Shows table and creates the Show's table.
        /// </summary>
        /// <param name="show"></param>
        public void CreateShow(Show show)
        {
            NonQuery($"create table [{show.Name}] (Season tinyint, [Episode #] tinyint, " +
                        "Title varchar(200), [Release Date] date, Watched bit, Downloaded bit)");

            NonQuery("insert into dbo.Shows (Name,ID,Seasons,Watched,Downloaded,Updatable,Channel) " +
                        $"values('{show.Name.Replace("'", "''")}','{show.Id}',{show.Seasons.Count}," +
                        $"0,0,0,'{show.Channel}')");

            foreach (Season season in show.Seasons)
            {
                AddSeason(show.Name, season);
            }
        }

        /// <summary>
        /// Gets episodes from a show within a season.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="seasonNumber"></param>
        /// <returns> List of Episodes </returns>
        public List<Episode> GetEpisodes(string title, byte seasonNumber)
        {
            List<Episode> episodes = new List<Episode>();
            DataTable dataTable = FillDataTable($"select * from dbo.[{title}] where Season = {seasonNumber} " +
                $"order by [Episode #]");

            foreach (DataRow row in dataTable.Rows)
            {
                episodes.Add(new Episode(row.Field<DateTime>("Release Date"), 
                    row.Field<bool>("Downloaded"),
                    row.Field<byte>("Episode #"), 
                    seasonNumber, 
                    row.Field<string>("Title"), 
                    row.Field<bool>("Watched")));
            }

            return episodes;
        }

        /// <summary>
        /// Gets seasons from a show.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="seasonsTotal"></param>
        /// <returns> List of Seasons </returns>
        public List<Season> GetSeasons(string title, int seasonsTotal)
        {
            List<Season> seasons = new List<Season>();

            for (byte season = 1; season <= seasonsTotal; season++)
            {
                List<Episode> episodes = GetEpisodes(title, season);

                seasons.Add(new Season(episodes.TrueForAll(i => i.Downloaded), episodes,
                    season, episodes.TrueForAll(i => i.Watched)));
            }

            return seasons;
        }

        /// <summary>
        /// Gets a show.
        /// </summary>
        /// <param name="title"></param>
        /// <returns> Show </returns>
        public Show GetShow(string title)
        {
            DataTable dataTable = FillDataTable($"select * from dbo.Shows " +
                $"where Name = '{title.Replace("'", "''")}'");

            DataRow row = dataTable.Rows[0];

            return new Show(row.Field<string>("Channel"), 
                row.Field<bool>("Downloaded"),
                row.Field<int>("ID"), 
                row.Field<string>("Name"),
                GetSeasons(title, row.Field<byte>("Seasons")), 
                row.Field<bool>("Updatable"),
                row.Field<bool>("Watched"), 
                this);
        }

        /// <summary>
        /// Gets all show Names.
        /// </summary>
        /// <returns> List of Shows </returns>
        public List<Show> GetShowNames()
        {
            List<Show> shows = new List<Show>();
            DataTable dataTable = FillDataTable("select * from dbo.Shows order by Name");

            foreach (DataRow row in dataTable.Rows)
            {
                shows.Add(new Show(row.Field<string>("Channel"), row.Field<string>("Name")));
            }

            return shows;
        }

        /// <summary>
        /// Gets all shows.
        /// </summary>
        /// <returns> List of Shows </returns>
        public List<Show> GetShows()
        {
            List<Show> shows = new List<Show>();
            DataTable dataTable = FillDataTable("select * from dbo.Shows order by Name");
            
            foreach (DataRow row in dataTable.Rows)
            {
                shows.Add(new Show(row.Field<string>("Channel"), row.Field<bool>("Downloaded"),
                row.Field<int>("ID"), row.Field<string>("Name"),
                GetSeasons(row.Field<string>("Name"), row.Field<byte>("Seasons")), row.Field<bool>("Updatable"),
                row.Field<bool>("Watched"), this));
            }

            return shows;
        }

        /// <summary>
        /// Gets all shows with a true value for updatable.
        /// </summary>
        /// <returns> List of updatable shows </returns>
        public List<Show> GetUpdateShows()
        {
            List<Show> shows = new List<Show>();
            DataTable dataTable = FillDataTable("select * from dbo.Shows where Updatable = 1 " +
                "order by Name");
            
            foreach (DataRow row in dataTable.Rows)
            {
                shows.Add(new Show(row.Field<string>("Channel"), row.Field<bool>("Downloaded"),
                row.Field<int>("ID"), row.Field<string>("Name"),
                GetSeasons(row.Field<string>("Name"), row.Field<byte>("Seasons")), row.Field<bool>("Updatable"),
                row.Field<bool>("Watched"), this));
            }

            return shows;
        }

        /// <summary>
        /// Removes a season's episodes from a show's table.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="season"></param>
        public void RemoveSeason(string title, int season)
        {
            NonQuery($"delete from dbo.[{title}] where Season = {season}");
        }

        /// <summary>
        /// Removes a show from the shows table and drops the show's table.
        /// </summary>
        /// <param name="title"></param>
        public void RemoveShow(string title)
        {
            NonQuery($"delete from dbo.Shows where Name = '{title.Replace("'", "''")}'");
            NonQuery($"drop table dbo.[{title}]");
        }

        /// <summary>
        /// Sets an episode's downloaded value to true;
        /// </summary>
        /// <param name="title"></param>
        /// <param name="episodeTitle"></param>
        public void SetDownloadEpisode(string title, string episodeTitle)
        {
            NonQuery($"update dbo.[{title}] set" +
                $" Downloaded = 1 where Title = '{episodeTitle.Replace("'", "''")}'");
        }

        /// <summary>
        /// Sets all episode's downloaded value to true for a season
        /// </summary>
        /// <param name="title"></param>
        /// <param name="seasonNumber"></param>
        public void SetDownloadSeason(string title, byte seasonNumber)
        {
            NonQuery($"update dbo.[{title}] set" +
                $" Downloaded = 1 where Season = {seasonNumber}");
        }

        /// <summary>
        /// Sets a show's downloaded value to true.
        /// </summary>
        /// <param name="title"></param>
        public void SetDownloadShow(string title)
        {
            NonQuery("update dbo.Shows set" +
                $" Downloaded = 1 where Name = '{title.Replace("'", "''")}'");
        }

        /// <summary>
        /// Sets a show's seasons total value.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="seasonNumber"></param>
        public void SetSeasonTotal(string title, int seasonNumber)
        {
            NonQuery($"update dbo.Shows set Seasons = {seasonNumber} " +
                $"where Name = '{title.Replace("'", "''")}'");
        }

        /// <summary>
        /// Sets a show's updatable value.
        /// </summary>
        /// <param name="show"></param>
        public void SetUpdatableShow(Show show)
        {
            byte update = (byte)(show.Update ? 0 : 1);
            NonQuery($"update dbo.Shows set Updatable = {update} " +
                $"where Name = '{show.Name.Replace("'", "''")}'");
        }

        /// <summary>
        /// Sets an episode's downloaded and watched values to true;
        /// </summary>
        /// <param name="title"></param>
        /// <param name="episodeTitle"></param>
        public void SetWatchEpisode(string title, string episodeTitle)
        {
            NonQuery($"update dbo.[{title}] set Watched = 1," +
                $" Downloaded = 1 where Title = '{episodeTitle.Replace("'", "''")}'");
        }

        /// <summary>
        /// Sets all episode's downloaded and watched values to true for a season
        /// </summary>
        /// <param name="title"></param>
        /// <param name="seasonNumber"></param>
        public void SetWatchSeason(string title, byte seasonNumber)
        {
            NonQuery($"update dbo.[{title}] set Watched = 1," +
                $" Downloaded = 1 where Season = {seasonNumber}");
        }

        /// <summary>
        /// Sets a show's downloaded and watched values to true.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="watched"></param>
        public void SetWatchShow(string title, bool watched)
        {
            byte update = (byte)(watched ? 1 : 0);
            NonQuery($"update dbo.Shows set Watched = {update}," +
                $" Downloaded = {update} where Name = '{title.Replace("'","''")}'");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a new episode to the show's table.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="episode"></param>
        private void AddEpisode(string title, Episode episode)
        {
            byte downloaded = (byte)(episode.Downloaded ? 1 : 0);
            byte watched = (byte)(episode.Watched ? 1 : 0);

            NonQuery($"insert into dbo.[{title}] (Season,[Episode #],Title," +
                        $"[Release Date],Watched,Downloaded) values({episode.SeasonNumber}," +
                        $"{episode.EpisodeNumber},'{TitleLengthCheck(episode.Title).Replace("'", "''")}'," +
                        $"'{episode.Released}',{watched},{downloaded})");
        }

        /// <summary>
        /// Executes SQL command and returns the result in a DataTable.
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <returns> DataTable of returned results </returns>
        private DataTable FillDataTable(string sqlStatement)
        {
            DataTable dataTable = new DataTable();
            SqlCommand sqlCommand = new SqlCommand(sqlStatement, SqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.Fill(dataTable);
            return dataTable;
        }
        
        /// <summary>
        /// Executes SQL command.
        /// </summary>
        /// <param name="sqlStatement"></param>
        private void NonQuery(string sqlStatement)
        {
            SqlCommand sqlCommand = new SqlCommand(sqlStatement, SqlConnection);
            sqlCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Check episode title for length and truncate if it is greater than 200 characters
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        private string TitleLengthCheck(string Title)
        {
            if (Title.Length > 200)
            {
                return Title.Substring(0, 200);
            }
            else
            {
                return Title;
            }
        }

        #endregion
    }
}
