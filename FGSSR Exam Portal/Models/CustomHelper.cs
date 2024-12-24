using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using EMSYS.Resources;

namespace EMSYS.CustomHelper
{
    public static class CustomHelper
    {
        public static IHtmlContent CustomDropDownList(string id, List<SelectListItem> selectListItems, string customPlaceHolder = "", string onChangeValueFunctionName = "")
        {
            string placeholderText = (string.IsNullOrEmpty(customPlaceHolder) ? Resource.PleaseSelect : customPlaceHolder);
            string selectedText = "";
            if (selectListItems!=null)
            {
                selectedText = selectListItems.Where(a => a.Selected == true).Select(a => a.Text).FirstOrDefault();
            }
            else
            {
                selectListItems = new List<SelectListItem>();
            }

            var buildDiv = new TagBuilder("div");
            buildDiv.AddCssClass("select-wrapper");

            var buildSelect = new TagBuilder("div");
            buildSelect.AddCssClass("select");
          
            var buildSelectTrigger = new TagBuilder("div");
            buildSelectTrigger.AddCssClass("select__trigger");

            var buildSelectSpan = new TagBuilder("span");
            buildSelectSpan.InnerHtml.Append(string.IsNullOrEmpty(selectedText) ? placeholderText : selectedText);

            var buildArrow = new TagBuilder("div");
            buildArrow.AddCssClass("arrow");

            var buildArrowIcon = new TagBuilder("i");
            buildArrowIcon.AddCssClass("fa-solid fa-caret-down");

            buildArrow.InnerHtml.AppendHtml(buildArrowIcon);
            buildSelectTrigger.InnerHtml.AppendHtml(buildSelectSpan);
            buildSelectTrigger.InnerHtml.AppendHtml(buildArrow);

            var buildCustomOptions = new TagBuilder("div");
            buildCustomOptions.AddCssClass("custom-options");

            var buildInput = new TagBuilder("input");
            buildInput.GenerateId(id, "_");
            buildInput.AddCssClass("custom-option");
            buildInput.Attributes.Add("name", id);
            buildInput.Attributes.Add("for", id);
            buildInput.Attributes.Add("hidden", "hidden");
            buildInput.Attributes.Add("value", "");

            var buildPlaceholder = new TagBuilder("span");
            buildPlaceholder.InnerHtml.Append(placeholderText);
            if (!string.IsNullOrEmpty(selectedText))
            {
                buildPlaceholder.AddCssClass("custom-option");
            }
            else
            {
                buildPlaceholder.AddCssClass("custom-option selected");
            }

            buildPlaceholder.Attributes.Add("value", null);
            buildPlaceholder.Attributes.Add("data-value", null);
            buildPlaceholder.Attributes.Add("data-text", "all");

            if (!string.IsNullOrEmpty(onChangeValueFunctionName))
            {
                buildPlaceholder.Attributes.Add("onclick", onChangeValueFunctionName + "(this)");
            }

            buildCustomOptions.InnerHtml.AppendHtml(buildPlaceholder);
            if (selectListItems!=null)
            {
                foreach (var item in selectListItems)
                {
                    var buildOption = new TagBuilder("span");
                    buildOption.Attributes.Add("value", item.Value);
                    buildOption.Attributes.Add("data-value", item.Value);
                    buildOption.Attributes.Add("data-text", item.Text);
                    buildOption.InnerHtml.Append(item.Text);
                    if (item.Selected == true)
                    {
                        buildInput.Attributes["value"] = item.Value;
                        buildOption.AddCssClass("custom-option selected");
                    }
                    else
                    {
                        buildOption.AddCssClass("custom-option");
                    }
                    if (!string.IsNullOrEmpty(onChangeValueFunctionName))
                    {
                        buildOption.Attributes.Add("onclick", onChangeValueFunctionName + "(this)");
                    }
                    buildCustomOptions.InnerHtml.AppendHtml(buildOption);
                }
            }

            buildSelect.InnerHtml.AppendHtml(buildInput);
            buildSelect.InnerHtml.AppendHtml(buildSelectTrigger);
            buildSelect.InnerHtml.AppendHtml(buildCustomOptions);

            buildDiv.InnerHtml.AppendHtml(buildSelect);
            return buildDiv;
        }

        public static IHtmlContent CustomRadioButton(string name, string id, string val, string label, string selectedVal, string defaultVal)
        {
            var buildDiv = new TagBuilder("div");
            buildDiv.Attributes.Add("class", "form-check form-check-inline");

            var buildInput = new TagBuilder("input");
            buildInput.GenerateId(id,"_");
            buildInput.Attributes.Add("class", "form-check-input");
            buildInput.Attributes.Add("type", "radio");
            buildInput.Attributes.Add("name", name);
            buildInput.Attributes.Add("for", id);
            buildInput.Attributes.Add("value", val.ToString());
            if (!string.IsNullOrEmpty(selectedVal))
            {
                if (val == selectedVal)
                {
                    buildInput.Attributes.Add("checked", "checked");
                }
            }
            else
            {
                if (val == defaultVal)
                {
                    buildInput.Attributes.Add("checked", "checked");
                }
            }

            var buildLabel = new TagBuilder("label");
            buildLabel.Attributes.Add("class", "form-check-label");
            buildLabel.Attributes.Add("for", id);
            buildLabel.InnerHtml.Append(label);

            buildDiv.InnerHtml.AppendHtml(buildInput);
            buildDiv.InnerHtml.AppendHtml(buildLabel);
            return buildDiv;
        }

        public static IHtmlContent CustomMultiSelect(string id, List<SelectListItem> selectListItems)
        {
            var buildDiv = new TagBuilder("div");

            var buildSelect = new TagBuilder("select");
            buildSelect.Attributes.Add("id", id);
            buildSelect.Attributes.Add("name", id);
            buildSelect.Attributes.Add("for", id);
            buildSelect.Attributes.Add("class", "multichoice");
            buildSelect.Attributes.Add("multiple", "multiple");
            buildSelect.Attributes.Add("placeholder", Resource.Selectmultiple);

            foreach (var item in selectListItems)
            {
                var buildOption = new TagBuilder("option");
                buildOption.Attributes.Add("value", item.Value);
                buildOption.InnerHtml.SetContent(item.Text);
                if (item.Selected)
                {
                    buildOption.Attributes.Add("selected", "selected");
                }

                buildSelect.InnerHtml.AppendHtml(buildOption);
            }

            buildDiv.InnerHtml.AppendHtml(buildSelect);

            return buildDiv;
        }


        public static Microsoft.AspNetCore.Html.HtmlString CustomMultiSelectFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var buildDiv = new TagBuilder("div");
            buildDiv.TagRenderMode = TagRenderMode.Normal;
            return new Microsoft.AspNetCore.Html.HtmlString(buildDiv.ToString());
        }

        public static IHtmlContent CustomSearchAndSelect(string id, string datalistId, List<SelectListItem> selectListItems)
        {
            var buildDiv = new TagBuilder("div");

            var buildInput = new TagBuilder("input");
            buildInput.Attributes.Add("id", id);
            buildInput.Attributes.Add("name", id);
            buildInput.Attributes.Add("for", id);
            buildInput.Attributes.Add("class", "form-control");
            buildInput.Attributes.Add("list", datalistId);
            buildInput.Attributes.Add("placeholder", Resource.TypetoSearch);

            var buildSelect = new TagBuilder("datalist");
            buildSelect.Attributes.Add("id", datalistId);

            foreach (var item in selectListItems)
            {
                var buildOption = new TagBuilder("option");
                buildOption.Attributes.Add("value", item.Value);
                buildOption.InnerHtml.SetContent(item.Text);
                if (item.Selected)
                {
                    buildInput.Attributes.Add("value", item.Value);
                    buildOption.Attributes.Add("selected", "selected");
                }

                buildSelect.InnerHtml.AppendHtml(buildOption);
            }

            buildDiv.InnerHtml.AppendHtml(buildInput);
            buildDiv.InnerHtml.AppendHtml(buildSelect);

            return buildDiv;
        }
    }
}