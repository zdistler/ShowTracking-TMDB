using Newtonsoft.Json;

namespace ShowTracking
{
    class APIRequest
    {
        #region Private Fields

        private readonly HttpClient Client;

        private readonly string Bearer;

        #endregion

        #region Constructor

        public APIRequest(string bearer)
        {
            Client = new HttpClient();
            Bearer = bearer;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// REST get call to search for shows by title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns> Results of search </returns>
        public async Task<SearchSeriesAPI> GetId(string title)
        {
            return await HttpRequest<SearchSeriesAPI>($"https://api.themoviedb.org/3/search/tv?query={title}&include_adult=true");
        }

        /// <summary>
        /// REST get call for the full season of the show.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="seasonNumber"></param>
        /// <returns> Season of show </returns>
        public async Task<Season?> GetSeason(int id, int seasonNumber)
        {
            try
            {
                Season season = await HttpRequest<Season>(
                    $"https://api.themoviedb.org/3/tv/{id}/season/{seasonNumber}");
                
                season.SetNumber((byte)seasonNumber);

                return season;
            }
            catch
            {
                return null;
            }
        }

        public async Task<SeriesResultAPI> GetShow(int id)
        {
            return await HttpRequest<SeriesResultAPI>($"https://api.themoviedb.org/3/tv/{id}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Runs the Get requests to the API.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns> Specified class based on JSON </returns>
        private async Task<T> HttpRequest<T>(string uri)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers =
                {
                    { "accept", "application/json" },
                    { "Authorization", $"Bearer {Bearer}" }
                }
            };
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new JsonConvertDateTime() }
            };
            using (HttpResponseMessage response = Client.Send(request))
            {
                response.EnsureSuccessStatusCode();
                string body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(body, settings);
            };
        }

        #endregion
    }
}
