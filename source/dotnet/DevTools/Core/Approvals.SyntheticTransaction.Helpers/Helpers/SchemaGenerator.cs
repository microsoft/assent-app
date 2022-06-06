// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NJsonSchema;

    /// <summary>
    /// Schema Generator class
    /// </summary>
    public class SchemaGenerator : ISchemaGenerator
    {
        /// <summary>
        /// Generate json schema
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public JsonSchema Generate(string json)
        {
            var token = JsonConvert.DeserializeObject<JToken>(json, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            });

            var schema = new JsonSchema();
            Generate(token, schema, schema, "Anonymous");
            return schema;
        }

        /// <summary>
        /// Generate json schema
        /// </summary>
        /// <param name="token"></param>
        /// <param name="schema"></param>
        /// <param name="rootSchema"></param>
        /// <param name="typeNameHint"></param>
        private void Generate(JToken token, JsonSchema schema, JsonSchema rootSchema, string typeNameHint)
        {
            GenerateWithoutReference(token, schema, rootSchema, typeNameHint);
        }

        /// <summary>
        /// Generate json schema without reference
        /// </summary>
        /// <param name="token"></param>
        /// <param name="schema"></param>
        /// <param name="rootSchema"></param>
        /// <param name="typeNameHint"></param>
        private void GenerateWithoutReference(JToken token, JsonSchema schema, JsonSchema rootSchema, string typeNameHint)
        {
            if (token == null)
            {
                return;
            }

            switch (token.Type)
            {
                case JTokenType.Object:
                    GenerateObject(token, schema, rootSchema);
                    break;

                case JTokenType.Array:
                    GenerateArray(token, schema, rootSchema, typeNameHint);
                    break;

                case JTokenType.Date:
                    schema.Type = JsonObjectType.String;
                    schema.Format = token.Value<DateTime>() == token.Value<DateTime>().Date
                        ? JsonFormatStrings.Date
                        : JsonFormatStrings.DateTime;
                    break;

                case JTokenType.String:
                    schema.Type = JsonObjectType.String;
                    break;

                case JTokenType.Boolean:
                    schema.Type = JsonObjectType.Boolean;
                    break;

                case JTokenType.Integer:
                    schema.Type = JsonObjectType.Integer;
                    break;

                case JTokenType.Float:
                    schema.Type = JsonObjectType.Number;
                    break;

                case JTokenType.Bytes:
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.Byte;
                    break;

                case JTokenType.TimeSpan:
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.TimeSpan;
                    break;

                case JTokenType.Guid:
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.Guid;
                    break;

                case JTokenType.Uri:
                    schema.Type = JsonObjectType.String;
                    schema.Format = JsonFormatStrings.Uri;
                    break;
            }

            if (schema.Type == JsonObjectType.String && Regex.IsMatch(token.Value<string>(), "^[0-2][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]$"))
            {
                schema.Format = JsonFormatStrings.Date;
            }

            if (schema.Type == JsonObjectType.String && Regex.IsMatch(token.Value<string>(), "^[0-2][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9](:[0-9][0-9])?$"))
            {
                schema.Format = JsonFormatStrings.DateTime;
            }

            if (schema.Type == JsonObjectType.String && Regex.IsMatch(token.Value<string>(), "^[0-9][0-9]:[0-9][0-9](:[0-9][0-9])?$"))
            {
                schema.Format = JsonFormatStrings.TimeSpan;
            }
        }

        /// <summary>
        /// Generate json object
        /// </summary>
        /// <param name="token"></param>
        /// <param name="schema"></param>
        /// <param name="rootSchema"></param>
        private void GenerateObject(JToken token, JsonSchema schema, JsonSchema rootSchema)
        {
            schema.Type = JsonObjectType.Object;
            foreach (var property in ((JObject)token).Properties())
            {
                var propertySchema = new JsonSchemaProperty();
                var propertyName = property.Value.Type == JTokenType.Array ? ConversionUtilities.Singularize(property.Name) : property.Name;
                var typeNameHint = ConversionUtilities.ConvertToUpperCamelCase(propertyName, true);

                Generate(property.Value, propertySchema, rootSchema, typeNameHint);
                schema.Properties[property.Name] = propertySchema;
            }
        }

        /// <summary>
        /// Generate json array
        /// </summary>
        /// <param name="token"></param>
        /// <param name="schema"></param>
        /// <param name="rootSchema"></param>
        /// <param name="typeNameHint"></param>
        private void GenerateArray(JToken token, JsonSchema schema, JsonSchema rootSchema, string typeNameHint)
        {
            schema.Type = JsonObjectType.Array;

            var itemSchemas = ((JArray)token).Select(item =>
            {
                var itemSchema = new JsonSchema();
                GenerateWithoutReference(item, itemSchema, rootSchema, typeNameHint);
                return itemSchema;
            }).ToList();

            if (itemSchemas.Count == 0)
            {
                schema.Item = new JsonSchema();
            }
            else if (itemSchemas.GroupBy(s => s.Type).Count() == 1)
            {
                MergeAndAssignItemSchemas(rootSchema, schema, itemSchemas, typeNameHint);
            }
            else
            {
                schema.Item = itemSchemas.First();
            }
        }

        /// <summary>
        /// Merge and assign item schemas
        /// </summary>
        /// <param name="rootSchema"></param>
        /// <param name="schema"></param>
        /// <param name="itemSchemas"></param>
        /// <param name="typeNameHint"></param>
        private void MergeAndAssignItemSchemas(JsonSchema rootSchema, JsonSchema schema, List<JsonSchema> itemSchemas, string typeNameHint)
        {
            var firstItemSchema = itemSchemas.First();
            var itemSchema = new JsonSchema
            {
                Type = firstItemSchema.Type
            };

            if (firstItemSchema.Type == JsonObjectType.Object)
            {
                foreach (var property in itemSchemas.SelectMany(s => s.Properties).GroupBy(p => p.Key))
                {
                    itemSchema.Properties[property.Key] = property.First().Value;
                }
            }
            schema.Item = itemSchema;
        }
    }
}
