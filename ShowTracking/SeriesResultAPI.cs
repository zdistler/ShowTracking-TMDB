using Newtonsoft.Json;

namespace ShowTracking
{
    class SeriesResultAPI
    {
        #region Public properties

        [JsonProperty("first_air_date")]
        public DateTime AirDate { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Network { get; set; }
        public List<Network> Networks { get; set; }
        public string Overview { get; set; }

        [JsonProperty("number_of_seasons")]
        public byte SeasonsTotal { get; set; }
        public string Status { get; set; }
        public int Year => AirDate.Year;

        #endregion

        #region Public Methods

        /// <summary>
        /// Choose the network associated with the show.
        /// </summary>
        /// <returns></returns>
        public string ChooseNetwork()
        {
            if (Status == "Canceled" || Status == "Ended")
            {
                return "";
            }

            foreach (Network network in Networks)
            {
                string answer = CorrectNetwork(network);
                switch (answer)
                {
                    case "y":
                        Network = network.Name;
                        return Network;
                    case "n":
                        continue;
                    case "none":
                    case "cancel":
                        return answer;
                }
            }

            return "";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Displays the name of a network for the user to select whether it is correct.
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        private string CorrectNetwork(Network network)
        {
            Console.WriteLine($"{network.Name} : Network {Networks.FindIndex(net => net.Name == network.Name) + 1} of {Networks.Count}");
            Console.Write("Is this correct (Y/N): ");
            string answer = Console.ReadLine().ToLower().Trim();
            switch (answer)
            {
                case "y":
                case "n":
                case "none":
                case "cancel":
                    return answer;
                default:
                    return CorrectNetwork(network);
            }
        }

        #endregion
    }
}
