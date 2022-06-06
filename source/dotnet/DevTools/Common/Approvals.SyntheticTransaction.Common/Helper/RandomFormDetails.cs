// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Helper
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class representing generation of random form data
    /// </summary>
    public class RandomFormDetails : IRandomFormDetails
    {
        /// <summary>
        /// Created form data
        /// </summary>
        /// <param name="strJson"> Json representing type and its value</param>
        /// <returns>Updated Json with random data</returns>
        public Dictionary<string, object> CreateFormData(string strJson)
        {
            try
            {
                Dictionary<string, object> randomFormData = new Dictionary<string, object>();
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(strJson);

                foreach (var tt in values)
                {
                    if (tt.Value == null)
                    {
                        randomFormData.Add(tt.Key, null);
                    }
                    else
                    {
                        if (tt.Value.GetType() == typeof(JObject) || tt.Value.GetType() == typeof(Object))
                        {
                            var values2 = GetObjectValues(JsonConvert.DeserializeObject<Dictionary<string, object>>(tt.Value.ToString()));
                            randomFormData.Add(tt.Key, values2);
                        }
                        else if (tt.Value.GetType() == typeof(JArray) || tt.Value.GetType() == typeof(Array))
                        {
                            var values4 = GetArrayValues(JsonConvert.DeserializeObject<JArray>(tt.Value.ToString()));
                            randomFormData.Add(tt.Key, values4);
                        }
                        else
                        {
                            randomFormData.Add(tt.Key, RandomValue(tt.Value));
                        }
                    }
                }
                return randomFormData;
            }
            catch (Exception ex)
            {
                throw new Exception("Summary object could not be populated", ex);
            }
        }

        /// <summary>
        /// Get array values
        /// </summary>
        /// <param name="values3"></param>
        /// <returns></returns>
        private JArray GetArrayValues(JArray values3)
        {
            JArray values4 = new JArray();
            foreach (var value1 in values3)
            {
                if (value1.GetType() == typeof(JObject) || value1.GetType() == typeof(Object))
                {
                    var values2 = GetObjectValues(JsonConvert.DeserializeObject<Dictionary<string, object>>(value1.ToString()));
                    var values5 = JObject.FromObject(values2);
                    values4.Add(values5);
                }
                else if (value1.GetType() == typeof(JArray) || value1.GetType() == typeof(Array))
                {
                    var values2 = GetArrayValues(JsonConvert.DeserializeObject<JArray>(value1.ToString()));
                    values4.Add(values2);
                }
                else
                {
                    values4.Add(RandomValue(value1));
                }
            }
            return values4;
        }

        /// <summary>
        /// Get object values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetObjectValues(Dictionary<string, object> values)
        {
            Dictionary<string, object> values2 = new Dictionary<string, object>();
            foreach (var value in values)
            {
                if (value.Value == null)
                {
                    values2.Add(value.Key, null);
                }
                else
                {
                    if (value.Value.GetType() == typeof(JObject) || value.Value.GetType() == typeof(Object))
                    {
                        var values3 = GetObjectValues(JsonConvert.DeserializeObject<Dictionary<string, object>>(value.Value.ToString()));
                        values2.Add(value.Key, values3);
                    }
                    else if (value.Value.GetType() == typeof(JArray) || value.Value.GetType() == typeof(Array))
                    {
                        var values3 = GetArrayValues(JsonConvert.DeserializeObject<JArray>(value.Value.ToString()));
                        values2.Add(value.Key, values3);
                    }
                    else if (IsValidJson(value.Value.ToString()))
                    {
                        var obj = JObject.Parse(value.Value.ToString());
                        if(obj.GetType()== typeof(JObject) || obj.GetType() == typeof(Object))
                        {
                            var values3 = GetObjectValues(JsonConvert.DeserializeObject<Dictionary<string, object>>(obj.ToString()));
                            values2.Add(value.Key, values3);
                        }
                    }
                    else
                    {
                        values2.Add(value.Key, RandomValue(value.Value));
                    }
                }
            }
            return values2;
        }

        /// <summary>
        /// Random value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object RandomValue(object value)
        {
            object result;
            switch (value.GetType().ToString())
            {
                case "String":
                    result = string.Join(" ", RandomDataGenerator.Words(1, true));
                    break;

                case "Int":
                    result = RandomDataGenerator.RandomInteger();
                    break;

                case "Double":
                    result = Convert.ToString(RandomDataGenerator.DecimalDigits(5, 3));
                    break;

                case "Boolean":
                    result = RandomDataGenerator.RandomBoolean();
                    break;

                case "Long":
                    result = RandomDataGenerator.RandomDigits(7);
                    break;

                case "Date":
                    result = RandomDataGenerator.RandomDay().ToString("yyyy-MM-dd");
                    break;

                case "Char":
                    result = RandomDataGenerator.RandomChar().ToString();
                    break;

                case "Blank":
                    result = "";
                    break;

                case null:
                    result = null;
                    break;

                default:
                    result = value;
                    break;
                    //throw new InvalidOperationException("unknown item type");
            }
            return result;
        }

        /// <summary>
        /// Check json is valid 
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception) //some other exception
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
        /// Generate random number
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int RandomNumber(int min = int.MinValue, int max = int.MaxValue)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            var val = random.Next(min, max >= int.MaxValue ? int.MaxValue : max + 1);
            return val;
        }
    }
}