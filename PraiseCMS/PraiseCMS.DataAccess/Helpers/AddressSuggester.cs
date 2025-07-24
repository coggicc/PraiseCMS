using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Helpers
{
    public static class AddressSuggester
    {
        //public static List<Suggestion> GetSuggestions(string query, string sessionToken)
        //{
        //    using (var client = new WebClient())
        //    {
        //        var url = string.Format("https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=geocode&key={1}{2}", query, ApplicationCache.Instance.GoogleConfiguration.MapsApiKey, !string.IsNullOrEmpty(sessionToken) ? "&sessiontoken=" + sessionToken : "");
        //        var response = client.DownloadData(url);
        //        var text = System.Text.UTF8Encoding.UTF8.GetString(response);
        //        var result = JsonConvert.DeserializeObject<SearchSuggestion>(text);

        //        return (from r in result.predictions
        //                where !string.IsNullOrEmpty(r.place_id)
        //                //where r.terms.Any(t => t.value == "United States")
        //                select new Suggestion()
        //                {
        //                    PlaceId = r.place_id,
        //                    Description = r.description,
        //                    Value = string.Join(",", r.terms.Select(x => x.value))
        //                }).ToList();
        //    }
        //}
    }

    #region Supporting Classes

    public class Suggestion
    {
        public string PlaceId { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
    }

    class MatchedSubstring
    {
        public int length { get; set; }
        public int offset { get; set; }
    }

    class Term
    {
        public int offset { get; set; }
        public string value { get; set; }
    }

    class Prediction
    {
        public string description { get; set; }
        public List<MatchedSubstring> matched_substrings { get; set; }
        public List<Term> terms { get; set; }
        public string id { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public List<string> types { get; set; }
    }

    class SearchSuggestion
    {
        public List<Prediction> predictions { get; set; }
        public string status { get; set; }
    }

    #endregion
}