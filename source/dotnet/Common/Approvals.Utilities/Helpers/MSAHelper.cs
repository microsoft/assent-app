// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers;

using System;
using System.Linq;
using AdaptiveCards;
using AdaptiveCards.Templating;
using Newtonsoft.Json.Linq;

/// <summary>
/// MSA Helper class
/// </summary>
public static class MSAHelper
{
    /// <summary>
    /// Formats for CSV.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>returns object</returns>
    public static object FormatForCSV(object obj)
    {
        if (!(obj is string))
            return obj;

        var text = (string)obj;

        if (text.Contains('"') || text.Contains(","))
        {
            text = text.Replace("\"", "\"\"");
            text = "\"" + text + "\"";
        }

        return text;
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
            if (jObject[parameter] != null && jObject[parameter].Type != JTokenType.Null && !String.IsNullOrEmpty(jObject[parameter].ToString()))
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
    /// Create adaptive payload from template and json data
    /// </summary>
    /// <param name="template">Adaptive Template</param>
    /// <param name="data">Json Data</param>
    /// <returns>Adaptive Payload string</returns>
    public static string CreateCard(string template, string data)
    {
        AdaptiveCardTemplate transformer = new AdaptiveCardTemplate(template);
        var context = new EvaluationContext
        {
            Root = data
        };
        string cardJson = transformer.Expand(context);
        return cardJson;
    }

    /// <summary>
    /// Create Adaptive Card object from Adaptive payload json
    /// </summary>
    /// <param name="card">Adaptive Payload Json string</param>
    /// <returns>Deserialized Adaptive Card</returns>
    public static AdaptiveCard CreateCard(string card)
    {
        AdaptiveCardParseResult result = AdaptiveCard.FromJson(card);
        AdaptiveCard adaptiveCard = result.Card;
        return adaptiveCard;
    }
}