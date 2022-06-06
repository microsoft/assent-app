// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Adaptive Card Helper class
    /// </summary>
    public static class AdaptiveCardHelper
    {
        private enum CardElement
        {
            TextBlock,
            Image
        }

        private enum Containers
        {
            Container,
            ColumnSet,
            Column,
            FactSet,
            Fact,
            ImageSet
        }

        /// <summary>
        /// GetContentContainer
        /// </summary>
        /// <param name="columnSets"></param>
        /// <param name="padding"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        public static JObject GetContainer(JArray columnSets, string id, bool isVisible = true, string padding = "small", string spacing = "large", string style = "default", string backgroundImage = "")
        {
            JObject container = new JObject();

            container.Add("type", Containers.Container.ToString());
            container.Add("padding", padding);
            container.Add("spacing", spacing);
            container.Add("id", id);
            container.Add("isVisible", isVisible);
            container.Add("items", columnSets);
            container.Add("style", style);
            if (!string.IsNullOrEmpty(backgroundImage))
            {
                container.Add("backgroundImage", backgroundImage);
            }

            return container;
        }

        /// <summary>
        /// GetColumnSets
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static JObject GetColumnSets(JArray columns, bool IsSeparator = false, string spacing = "none", string padding = "none")
        {
            JObject container = new JObject();

            container.Add("type", Containers.ColumnSet.ToString());
            container.Add("separator", IsSeparator);
            container.Add("columns", columns);
            container.Add("spacing", spacing);
            container.Add("padding", padding);

            return container;
        }

        /// <summary>
        /// GetColumns
        /// </summary>
        /// <param name="items"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static JArray GetColumns(JArray items, ref JArray columns, string width = "auto", string verticalContentAlignment = "")
        {
            JObject container = new JObject();

            container.Add("type", Containers.Column.ToString());
            container.Add("width", width);
            container.Add("items", items);
            if (!string.IsNullOrEmpty(verticalContentAlignment))
            {
                container.Add("verticalContentAlignment", verticalContentAlignment);
            }
            columns.Add(container);

            return columns;
        }

        /// <summary>
        /// GetItems
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="items"></param>
        /// <param name="isSubtle"></param>
        /// <returns></returns>
        public static JArray GetItems(string type, string value, ref JArray items, bool isSubtle = false, string spacing = "none", string size = "Small", bool isProfilePicture = true, string horizontalAlignment = "", string padding = "", JObject styles = null, string weight = "Lighter", string width = "auto", bool wrap = true)
        {
            JObject container = new JObject();

            container.Add("type", type);

            if (type == "Image")
            {
                container.Add("url", value);
                if (isProfilePicture)
                {
                    container.Add("style", "person");
                    container.Add("pixelHeight", 35);
                }
                else
                {
                    container.Add("pixelWidth", 16);
                    container.Add("horizontalAlignment", "center");
                }
            }
            else
            {
                container.Add("text", value);

                if (styles != null && styles.Count > 0)
                {
                    foreach (var item in styles)
                    {
                        container.Add(item.Key, item.Value);
                    }
                }
                else
                {
                    container.Add("width", width);
                    container.Add("wrap", wrap);
                    container.Add("isSubtle", isSubtle);
                    container.Add("spacing", spacing);
                    container.Add("weight", weight);
                    if (string.IsNullOrEmpty(padding))
                    {
                        container.Add("padding", padding);
                    }
                    container.Add("size", size);
                    if (!string.IsNullOrEmpty(horizontalAlignment))
                    {
                        container.Add("horizontalAlignment", horizontalAlignment);
                    }
                }
            }

            items.Add(container);

            return items;
        }

        /// <summary>
        /// Build Up Down Arrow for line items
        /// </summary>
        /// <param name="index">1</param>
        /// <param name="direction">Down/Up</param>
        /// <param name="isVisible">true/false</param>
        /// <param name="columns">ref columns array</param>
        /// <returns></returns>
        public static JArray GetUpDownArrowColumn(string index, string direction, bool isVisible, ref JArray columns, string token, string url)
        {
            JObject container = new JObject();

            container.Add("id", token + direction + index);

            JObject selectAction = new JObject();
            selectAction.Add("type", "Action.ToggleVisibility");

            JArray targetElements = new JArray();

            string lineItemId = token + index.ToString();
            targetElements.Add(lineItemId.ToLowerInvariant());
            targetElements.Add(token + "Up" + index.ToString());
            targetElements.Add(token + "Down" + index.ToString());

            selectAction.Add("targetElements", targetElements);

            container.Add("selectAction", selectAction);

            container.Add("isVisible", isVisible);
            container.Add("width", "auto");
            container.Add("verticalContentAlignment", "center"); //center

            JArray items = new JArray();
            JObject item = new JObject();

            item.Add("type", "Image");
            item.Add("pixelWidth", 12);
            item.Add("url", url);

            items.Add(item);

            container.Add("items", items);

            columns.Add(container);

            return columns;
        }

        /// <summary>
        /// Build Up Down Arrow for line items
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="isVisible">true/false</param>
        /// <param name="columns">ref columns array</param>
        /// <param name="backgroundImage"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <param name="title"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static JArray GetShowHideButtonColumn(string id, string status, bool isVisible, ref JArray columns, string backgroundImage, string title, string width = "auto", string size = "large", string color = "light")
        {
            JObject container = new JObject();

            container.Add("id", id + status);

            JObject selectAction = new JObject();
            selectAction.Add("type", "Action.ToggleVisibility");

            JArray targetElements = new JArray();

            targetElements.Add(id);
            targetElements.Add(id + "Show");
            targetElements.Add(id + "Hide");

            selectAction.Add("targetElements", targetElements);
            container.Add("backgroundImage", backgroundImage);
            container.Add("selectAction", selectAction);

            container.Add("isVisible", isVisible);
            container.Add("width", width);
            container.Add("verticalContentAlignment", "center");

            JArray items = new JArray();
            JObject item = new JObject();

            item.Add("type", "TextBlock");
            item.Add("color", color);
            item.Add("text", title);
            item.Add("size", size);
            item.Add("wrap", true);

            items.Add(item);

            container.Add("items", items);

            columns.Add(container);

            return columns;
        }

        /// <summary>
        /// Get Action Card
        /// </summary>
        /// <param name="buttonTitle"></param>
        /// <param name="inputs"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static JObject ActionsCard(string buttonTitle, JArray inputs, JArray actions)
        {
            JObject action = new JObject();

            action.Add("title", buttonTitle);
            action.Add("type", "Action.ShowCard");
            action.Add("card", CardDetails(inputs, actions));

            return action;
        }

        /// <summary>
        /// Get Action Card
        /// </summary>
        /// <param name="buttonTitle"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static JObject ActionsCard(string buttonTitle, string url)
        {
            JObject action = new JObject();

            action.Add("title", buttonTitle);
            action.Add("type", "Action.OpenUrl");
            action.Add("url", url);

            return action;
        }

        /// <summary>
        /// Get Action Set
        /// </summary>
        /// <param name="actions"></param>
        /// <param name="id"></param>
        /// <param name="horizontalAlignment"></param>
        /// <returns></returns>
        public static JObject ActionSet(JArray actions, string id, string horizontalAlignment = "left")
        {
            JObject action = new JObject();

            action.Add("type", "ActionSet");
            action.Add("id", id);
            action.Add("horizontalAlignment", horizontalAlignment);
            action.Add("actions", actions);

            return action;
        }

        /// <summary>
        /// Get Card Details
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public static JObject CardDetails(JArray inputs, JArray actions)
        {
            JObject card = new JObject();
            if (actions != null && actions.Count > 0)
            {
                card.Add("actions", actions);
            }
            card.Add("body", inputs);
            card.Add("type", "AdaptiveCard");

            return card;
        }

        /// <summary>
        /// Get Input controls
        /// </summary>
        /// <param name="title"></param>
        /// <param name="Id"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static JArray InputControls(string title, string Id, ref JArray inputs)
        {
            JObject control = new JObject();

            control.Add("type", "Input.ChoiceSet");
            control.Add("id", Id);
            control.Add("isRequired", true);
            control.Add("isMultiSelect", true);

            JArray choiceItems = new JArray();
            JObject choiceItem = new JObject
            {
                { "title", title },
                { "value", "false" }
            };

            choiceItems.Add(choiceItem);

            control.Add("choices", choiceItems);

            inputs.Add(control);

            return inputs;
        }

        /// <summary>
        /// Setting the FactSet for controls.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="inputFactSets"></param>
        /// <returns>JObject with FactSet control properties</returns>
        public static JObject FactSetControls(string Id, JArray inputFactSets)
        {
            JObject control = new JObject();
            control.Add("type", "FactSet");
            control.Add("id", Id);
            control.Add("facts", inputFactSets);

            return control;
        }
    }
}