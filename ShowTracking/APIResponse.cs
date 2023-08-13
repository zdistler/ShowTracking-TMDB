namespace ShowTracking
{
    class APIResponse
    {
        #region Public Properties

        public string ErrorMessage { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if an error message was sent with the API response.
        /// </summary>
        /// <returns> If there is an error message </returns>
        public bool ErrorCheck()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                if (ErrorMessage.Contains("Maximum usage"))
                {
                    Console.WriteLine(ErrorMessage);
                }

                return true;
            }
            return false;
        }

        #endregion
    }
}
