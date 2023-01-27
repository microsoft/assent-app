// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class JSONHelper.
/// </summary>
public static class JSONHelper
{
    /// <summary>
    /// Serialize an Object into JSON string
    /// </summary>
    /// <typeparam name="T">Class name</typeparam>
    /// <param name="obj">Object class to be serialized</param>
    /// <returns>Json string </returns>
    public static string ConvertObjectToJSON<T>(T obj)
    {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
        MemoryStream ms = new MemoryStream();
        ser.WriteObject(ms, obj);
        string jsonString = Encoding.UTF8.GetString(ms.ToArray());
        ms.Close();
        return jsonString;
    }

    /// <summary>
    /// Replace the configurable items in a string
    /// </summary>
    // Used to form a Url to fetch data from Pull Tenant with or without filtering Criteria
    public static string ReplacePlaceholder(string valueWithPlaceholder, IDictionary<string, object> parameters, string placeholderStart, string placeholderEnd)
    {
        return Regex.Replace(
        valueWithPlaceholder,
        placeholderStart + "(.+?)" + placeholderEnd,
        match => parameters.ContainsKey(match.Groups[1].Value.Replace(placeholderStart, string.Empty).Replace(placeholderEnd, string.Empty)) ?
            parameters[match.Groups[1].Value].ToString() : string.Empty);
    }

    /// <summary>
    /// Deserialize a json string into Object
    /// </summary>
    /// <typeparam name="T">class name</typeparam>
    /// <param name="strJson">json string which needs to be deserialized</param>
    /// <returns></returns>
    public static T ConvertJSONToObject<T>(string strJson)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strJson));
        T obj = (T)serializer.ReadObject(ms);
        return obj;
    }

    /// <summary>
    /// Updates a dictionary to store the placeholders which needs to be replaced with specific values
    /// </summary>
    /// <param name="placeHolderDict">The place holder dictionary.</param>
    /// <param name="summaryData">The summary data.</param>
    /// <param name="dataplaceHolder">The dataplace holder.</param>
    public static void ConvertJsonToDictionary(Dictionary<string, string> placeHolderDict,
        string summaryData,
        string dataplaceHolder = "")
    {
        try
        {
            var values = summaryData.FromJson<Dictionary<string, object>>();
            if (values == null)
            {
                return;
            }
            foreach (KeyValuePair<string, object> data in values)
            {
                string placeHolder = String.IsNullOrEmpty(dataplaceHolder) ? data.Key : dataplaceHolder + "." + data.Key;

                string dataVal = data.Value?.ToString();
                if (data.Value == null)
                {
                    //// if the value is null. Add the values to dictionary.
                    placeHolderDict.Add(placeHolder, null);
                }
                else
                {
                    //// First we need to verify for Valid JSON before going into recurrsion.
                    if (dataVal.IsJson())
                    {
                        //// call this function on recurrsion if nested JSON structure is found.
                        ConvertJsonToDictionary(placeHolderDict, data.Value.ToJson(), placeHolder);
                        continue;
                    }
                    else
                    {
                        //// If both key and values are not null. Add both Key and Values to dictionary.
                        placeHolderDict.Add(placeHolder, dataVal);
                    }
                }
            }
        }
        catch
        {
            // extra check to abort the loop in case there is any problem in serialization/ deserialization in above logic.
            return;
        }
    }

    /// <summary>
    /// Converts the j array to dictionary.
    /// </summary>
    /// <param name="documentKeys">The document keys.</param>
    /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
    public static Dictionary<string, string> ConvertJArrayToDictionary(JArray documentKeys)
    {
        return documentKeys.ToDictionary(element => element["Key"].ToString(), (element) => element["Value"].ToString());
    }

    /// <summary>
    /// Validates the json response.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="response">The response.</param>
    /// <param name="serviceDownException">The service down exception.</param>
    /// <returns>returns System.Threading.Tasks</returns>
    public static async Task<JObject> ValidateJsonResponse(IConfiguration config, HttpResponseMessage response, string serviceDownException)
    {
        string responseString = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.OK)
            throw new WebException("Status Code: " + response.StatusCode.ToString() + " " + responseString, WebExceptionStatus.ReceiveFailure);

        return ValidateJsonString(config, responseString, serviceDownException + " relay service is down");
    }

    /// <summary>
    /// Handles NotFound status code and returns empty JObject
    /// </summary>
    /// <returns>returns empty JObject incase of 404 NotFound status</returns>
    public static async Task<JObject> HandleNotFound()
    {
        HttpResponseMessage response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") };
        string responseString = await response.Content.ReadAsStringAsync();
        return responseString.ToJObject();
    }

    /// <summary>
    /// Validates the json string.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="responseString">The response string.</param>
    /// <param name="serviceDownException">The service down exception.</param>
    /// <returns>returns Newtonsoft.Json.Linq.JObject</returns>
    public static JObject ValidateJsonString(IConfiguration config, string responseString, string serviceDownException)
    {
        if (responseString.IsJson())
        {
            return responseString.ToJObject();
        }
        else
        {
            if (responseString.Contains(config[ConfigurationKey.RelayServiceDownMessage.ToString()]))
            {
                throw new WebException(serviceDownException, WebExceptionStatus.ReceiveFailure);
            }
            else
            {
                throw new WebException("String is not valid JSON. String: " + responseString, WebExceptionStatus.ReceiveFailure);
            }
        }
    }

    /// <summary>
    /// Gets the value from inner json.
    /// </summary>
    /// <param name="actionObject">The action object.</param>
    /// <param name="property">The property.</param>
    /// <param name="innerProperty">The inner property.</param>
    /// <returns>returns System.String.</returns>
    public static string GetValueFromInnerJson(JObject actionObject, string property, string innerProperty)
    {
        JObject documentObject = actionObject[property]?.ToString().FromJson<JObject>();
        string documentNumber = documentObject[innerProperty].ToString();
        return documentNumber;
    }

    /// <summary>
    /// Extracts the value from json.
    /// </summary>
    /// <param name="jObject">The j object.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <returns>returns string</returns>
    public static string ExtractValueFromJSON(JObject jObject, string parameterName)
    {
        var value = string.Empty;

        var parameters = parameterName.Split('.');

        foreach (var parameter in parameters.Take(parameters.Count() - 1))
        {
            if (jObject[parameter] != null && jObject[parameter].Type != JTokenType.Null && !string.IsNullOrWhiteSpace(jObject[parameter]?.ToString()))
            {
                jObject = (JObject)jObject[parameter];
            }
            else
                break;
        }
        if (parameters.LastOrDefault() != null && jObject[parameters.LastOrDefault()] != null && jObject[parameters.LastOrDefault()].Type != JTokenType.Null)
        {
            value = jObject[parameters.LastOrDefault()].ToString();
        }
        return value;
    }

    /// <summary>
    /// Checks whether the input string is in JSON format and if yes extracts the Message from the JSON string.
    /// </summary>
    /// <param name="strResponse"></param>
    /// <returns></returns>
    public static string ExtractMessageFromJSON(string jsonData)
    {
        string strMsg = string.Empty;
        if (jsonData.IsJson())
        {
            try
            {
                JObject jObj = JObject.Parse(jsonData);
                strMsg = jObj != null && jObj.TryGetValue("message", out JToken jToken) ? jToken.ToString() : string.Empty;
            }
            catch
            {
                //do nothing strErrorMsg will contain empty string
            }
        }
        return strMsg;
    }

    /// <summary>
    /// Replaces the placeholders in the string format with the dictionary values
    /// </summary>
    /// <param name="strResponse"></param>
    /// <returns></returns>
    public static string StringFormat(string format, IDictionary<string, string> values)
    {
        // TODO :: Need to cleanup for ApproverNotes and Notes to remove this if condition to replace placehoder value
        if (!values.ContainsKey("ApproverNotes") || string.IsNullOrWhiteSpace(values["ApproverNotes"]))
        {
            format = format.Replace("#Notes#", "&nbsp;");
        }
        else
        {
            format = format.Replace("#Notes#", "Notes: ");
        }

        foreach (var p in values)
            format = format.Replace("#" + p.Key + "#", p.Value);

        format = ReplacePlaceHoder(format);
        return format;
    }

    /// <summary>
    /// Replace all the instance of old value of property with new value in entire Json string
    /// </summary>
    public static string ReplaceInJsonString<T1, T2>(string jsonString, string propertyName, T1 oldValue, T2 newValue, bool considerCaseSensitivePropertyName = false)
    {
        var replacedValue = Regex.Replace(jsonString, "\"" + propertyName + "\"" + @"[:\s]" + oldValue, "\"" + propertyName + "\":" + newValue);
        if (!considerCaseSensitivePropertyName)
        {
            replacedValue = Regex.Replace(jsonString, "\"" + propertyName.ToLowerInvariant() + "\"" + @"[:\s]" + oldValue, "\"" + propertyName + "\":" + newValue);
            replacedValue = Regex.Replace(jsonString, "\"" + propertyName.ToUpperInvariant() + "\"" + @"[:\s]" + oldValue, "\"" + propertyName + "\":" + newValue);
            replacedValue = Regex.Replace(jsonString, "\"" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(propertyName) + "\"" + @"[:\s]" + oldValue, "\"" + propertyName + "\":" + newValue);
            replacedValue = Regex.Replace(jsonString, "\"" + char.ToLower(propertyName[0]) + propertyName.Substring(1) + "\"" + @"[:\s]" + oldValue, "\"" + propertyName + "\":" + newValue);
        }

        return replacedValue;
    }

    /// <summary>
    /// Replace unwanted placehodler from the response string.
    /// </summary>
    /// <param name="format">The format</param>
    /// <returns>Adaptive card response string.</returns>
    private static string ReplacePlaceHoder(string format)
    {
        string pattern = @"#(\S+)#";
        Regex r = new Regex(pattern);
        MatchCollection m = r.Matches(format);
        foreach (Match match in m)
        {
            format = format.Replace(match.Value, string.Empty);
        }
        return format;
    }
}