// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[ExcludeFromCodeCoverage]
public static class ExtensionMethods
{
    /// <summary>
    /// To the JObject.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>returns an JObject</returns>
    public static JObject ToJObject(this string value)
    {
        return JObject.Parse(value);
    }

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
}
