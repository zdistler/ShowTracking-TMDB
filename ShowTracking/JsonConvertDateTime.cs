using Newtonsoft.Json;

namespace ShowTracking
{
    class JsonConvertDateTime : JsonConverter<DateTime>
    {
        /// <summary>
        /// Checks if a value can be converted to DateTime. If yes, return it as a DateTime object.
        /// If no, return a DateTime object for tomorrow.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="hasExistingValue"></param>
        /// <param name="serializer"></param>
        /// <returns> DateTime object </returns>
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                if (DateTime.TryParse(reader.Value.ToString(), out _))
                {
                    return DateTime.Parse(reader.Value.ToString());
                }
                else
                {
                    return DateTime.Now.AddDays(1);
                }
            }
            catch
            {
                return DateTime.Now.AddDays(1);
            }

        }

        /// <summary>
        /// Not implemented. No JSON is written.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteJson(JsonWriter writer, DateTime value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
