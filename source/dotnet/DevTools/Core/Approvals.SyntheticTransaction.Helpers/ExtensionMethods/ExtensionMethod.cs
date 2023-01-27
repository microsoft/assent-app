// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Extension Methods class
/// </summary>
public static class ExtensionMethod
{
    /// <summary>
    /// Get string value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string StringValue<T>(this T val) where T : struct
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    /// <summary>
    /// Get json from object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T FromJson<T>(this object obj)
    {
        return JsonConvert.DeserializeObject<T>(obj as string);
    }

    /// <summary>
    /// Get JToken from object
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JToken ToJToken(this object value)
    {
        return JToken.FromObject(value);
    }

    /// <summary>
    /// Get JObject from string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static JObject ToJObject(this string value)
    {
        return JObject.Parse(value);
    }

    /// <summary>
    /// Add or Update Dictionary with particular key and value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="logData"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddUpdate<TKey, TValue>(this Dictionary<TKey ,TValue> logData, TKey key, TValue value)
    {
        if(!logData.TryAdd(key, value))
        {
            logData[key] = value;
        }
    }
}