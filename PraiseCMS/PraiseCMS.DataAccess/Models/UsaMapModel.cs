using System.Collections.Generic;

namespace PraiseCMS.Web.Helpers
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class UsMapRoot
    {
        public int Width { get; set; }
        public int Height { get; set; }
        //public Paths paths { get; set; }
        public StateMap Paths { get; set; }
    }

    public class StateMap
    {
        public Dictionary<string, StateInfo> States { get; set; }
    }

    public class StateInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }
}