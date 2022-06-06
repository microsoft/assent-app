// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Extensions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Extensions Standard class
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get correlation Id
        /// </summary>
        /// <param name="bindingData"></param>
        /// <returns></returns>
        public static string GetCorrelationId(this IReadOnlyDictionary<string, object> bindingData)
        {
            if (bindingData != null)
            {
                if (bindingData.ContainsKey("UserProperties") && !bindingData["UserProperties"].ToString().FromJson<Dictionary<string, string>>().ContainsKey("CorrelationID"))
                {
                    return bindingData["MessageId"].ToString();
                }
                return bindingData["UserProperties"].ToString().FromJson<Dictionary<string, string>>()["CorrelationID"];
            }
            else
                return "";
        }

        /// <summary>
        /// Get correlation Id
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetCorrelationId(this Message message)
        {
            if (message != null)
            {
                if (null != message.UserProperties && !message.UserProperties.ContainsKey("CorrelationID"))
                {
                    message.UserProperties.Add("CorrelationID", message.MessageId);
                }
                return message.UserProperties["CorrelationID"].ToString();
            }
            else
                return "";
        }

        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>returns an object</returns>
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Froms the json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static T FromJson<T>(this object obj)
        {
            return JsonConvert.DeserializeObject<T>(obj as string);
        }

        /// <summary>
        /// Deserializes the helper.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object FromJson(this string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }

        /// <summary>
        /// This is extension method on the dictionary to check if key present then overwrite it else add to dictionary
        /// </summary>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="key">The Key.</param>
        /// <param name="value">The Value.</param>
        public static void Modify<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// To the JObject.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>returns an JObject</returns>
        public static JObject ToJObject(this string value)
        {
            return JObject.Parse(value);
        }

        /// <summary>
        /// Deserializes the helper.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>returns an object</returns>
        public static T FromJson<T>(this string json, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        /// <summary>
        /// Deserializes the helper.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>returns an object</returns>
        public static object FromJson(this string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        /// <summary>
        /// Determines whether the specified json data is json.
        /// </summary>
        /// <param name="jsonData">The json data.</param>
        /// <returns>System.Boolean.</returns>
        public static bool IsJson(this string jsonData)
        {
            string data = jsonData.Trim();
            return !string.IsNullOrEmpty(data) && jsonData.IsValidJson();
        }

        /// <summary>
        /// Determines whether [is valid json] [the specified string input].
        /// </summary>
        /// <param name="strInput">The string input.</param>
        /// <returns>
        ///   <c>true</c> if [is valid json] [the specified string input]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidJson(this string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) return false;
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    JContainer.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// to verify if it is a valid JSON Array
        /// </summary>
        /// <param name="jsonData">string containing json data</param>
        /// <returns>returns boolean value</returns>
        public static bool IsJsonArray(this string jsonData)
        {
            try
            {
                (jsonData).ToJArray();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts object to JToken.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A JToken object created from the input object</returns>
        public static JToken ToJToken(this object value)
        {
            // Sending Octal numbers (with leading zeros) is not as per the specification of JSON
            // refer http://www.ietf.org/rfc/rfc4627.txt
            // This new extension method handles this scenario where the input value is sent as an object instead of string.
            // Now this input can be used for creating a JToken using the JToken.FromObject method which doesn't change the type of the input value
            return JToken.FromObject(value);
        }

        /// <summary>
        /// To the j array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>returns JArray object</returns>
        public static JArray ToJArray(this string value)
        {
            return JArray.Parse(value);
        }

        /// <summary>
        /// To the j token.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static JToken ToJToken(this string value)
        {
            return JToken.Parse(value);
        }

        /// <summary>
        /// To the json.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>returns an object</returns>
        public static string ToJson(this object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// Replace sql special characters
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public static string ReplaceSqlSpecialCharacters(this string searchCriteria)
        {
            if (string.IsNullOrEmpty(searchCriteria))
            {
                return string.Empty;
            }
            if (searchCriteria.Contains("'"))
            {
                searchCriteria = searchCriteria.Replace("'", "''");
            }

            var sqlSpecialCharacters = new string[] { "[", "_", "%" };

            foreach (string specialCharacter in sqlSpecialCharacters)
            {
                if (searchCriteria.Contains(specialCharacter))
                {
                    searchCriteria = searchCriteria.Replace(specialCharacter, string.Format("[{0}]", specialCharacter));
                }
            }
            return searchCriteria;
        }
    }
}