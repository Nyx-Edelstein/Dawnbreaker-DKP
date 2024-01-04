using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Dawnbreaker_DKP.Extensions
{
    public static class TempDataDictionaryExtensions
    {
        public const string REDIRECT_KEY = @"Redirect";
        public const string MESSAGE_KEY = @"Message";
        public const string ERROR_KEY = @"Errors";

        public static void SetRedirect(this ITempDataDictionary tempData, string redirectUrl) => Add(tempData, REDIRECT_KEY, redirectUrl);
        public static string GetRedirect(this ITempDataDictionary tempData) => tempData.ContainsKey(REDIRECT_KEY) ? (string)tempData[REDIRECT_KEY] : null;

        public static void AddMessage(this ITempDataDictionary tempData, string msg) => AddToList(tempData, MESSAGE_KEY, msg);
        public static List<string> GetMessages(this ITempDataDictionary tempData) => tempData.ContainsKey(MESSAGE_KEY) ? ((IEnumerable<string>)tempData[MESSAGE_KEY]).ToList() : new List<string>();

        public static void AddError(this ITempDataDictionary tempData, string error) => AddToList(tempData, ERROR_KEY, error);
        public static List<string> GetErrors(this ITempDataDictionary tempData) => tempData.ContainsKey(ERROR_KEY) ? ((IEnumerable<string>)tempData[ERROR_KEY]).ToList() : new List<string>();

        private static void AddToList<T>(this ITempDataDictionary tempData, string key, T msg)
        {
            if (!tempData.ContainsKey(key))
                tempData.Add(key, new List<T>());

            var existingData = ((IEnumerable<T>) tempData[key]).ToList();
            existingData.Add(msg);
            tempData[key] = existingData;
        }

        private static void Add<T>(ITempDataDictionary tempData, string key, T data)
        {
            if (!tempData.ContainsKey(key))
                tempData.Add(key, data);
            else
                tempData[key] = data;
        }
    }
}
