using CamelDotNet.Models;
using CamelDotNet.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc.Ajax;
using System.Web.Routing;
namespace System.Web.Mvc.Html
{
    public static class HtmlHelperExtensions
    {
        private const string unsort = "↕";
        private const string asc = "↑";
        private const string desc = "↓";

        //public static string ToTraceString<T>(this IQueryable<T> t)
        //{
        //    string sql = "";
        //    ObjectQuery<T> oqt = t as ObjectQuery<T>;
        //    if (oqt != null)
        //        sql = oqt.ToTraceString();
        //    return sql;
        //}

        public static object IndexPageInit(this HtmlHelper htmlHelper)
        {
            htmlHelper.ViewBag.Action = (((RouteValueDictionary)(htmlHelper.ViewBag.RV))["actionAjax"]).ToString();
            htmlHelper.ViewBag.ReturnRoot = (((RouteValueDictionary)(htmlHelper.ViewBag.RV))["returnRoot"]).ToString();
            var filter = ((RouteValueDictionary)(htmlHelper.ViewBag.RV))["filter"];
            if (filter != null && filter != "")
            {
                var filterStr = filter.ToString();
                var conditions = filterStr.Substring(0, filterStr.Length - 1).Split(';');
                foreach (var item in conditions)
                {
                    var tmp = item.Split(':');
                    htmlHelper.ViewData.Add(tmp[0], tmp[1]);
                }
            }

            var wvp = (WebViewPage)htmlHelper.ViewDataContainer;

            htmlHelper.ViewBag.AjaxOpts = new AjaxOptions
            {
                UpdateTargetId = "AjaxBody",
                Url = wvp.Url.Action(htmlHelper.ViewBag.Action),
                OnSuccess = "syncSuccess",
                OnFailure = "syncFail",
            };
            return null;
        }

        public static string getCurSort(this HtmlHelper helper, string sortFilter, string keyword)
        {
            string tmpAsc = keyword + "Asc";
            string tmpDesc = keyword + "Desc";

            if (sortFilter == tmpAsc)
            {
                return asc;
            }
            else if (sortFilter == tmpDesc)
            {
                return desc;
            }
            else
            {
                return unsort;
            }
        }

        public static string getDesSort(this HtmlHelper helper, string sortFilter, string keyword)
        {
            string tmpAsc = keyword + "Asc";
            string tmpDesc = keyword + "Desc";

            if (sortFilter == tmpAsc)
            {
                return keyword + "Desc";
            }
            else if (sortFilter == tmpDesc)
            {
                return keyword + "Asc";
            }
            else
            {
                return keyword + "Asc";
            }
        }

        public static IHtmlString LinkToRemoveNestedForm(this HtmlHelper htmlHelper, string linkText, string container, string deleteElement)
        {

            var js = string.Format("javascript:removeNestedForm(this,'{0}','{1}');return false;", container, deleteElement);

            TagBuilder tb = new TagBuilder("a");

            tb.Attributes.Add("href", "#");

            tb.Attributes.Add("onclick", js);

            tb.Attributes.Add("class", "remove");

            tb.InnerHtml = linkText;

            var tag = tb.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(tag);

        }

        public static IHtmlString LinkToRealRemoveNestedForm(this HtmlHelper htmlHelper, string linkText, string container)
        {

            var js = string.Format("javascript:realRemoveNestedForm(this,'{0}');return false;", container);

            TagBuilder tb = new TagBuilder("a");

            tb.Attributes.Add("href", "#");

            tb.Attributes.Add("onclick", js);

            tb.InnerHtml = linkText;

            var tag = tb.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(tag);

        }

        public static MvcHtmlString ReplacePerConfigEditPartailNull(this HtmlHelper helper, MvcHtmlString initModelContext, string linkText, string parentContainerElement, string containerElement, string counterElement, string parentProperty, string collectionProperty, string tick = "") 
        {
            var ticksParent = DateTime.UtcNow.Ticks;
            var partial = initModelContext.ToHtmlString().JsEncode();
            partial = partial.Replace(tick,"");
            partial = partial.Replace("name=\\\"", "name=\\\"" + parentProperty + "[" + ticksParent + "].");
            partial = partial.Replace("data-valmsg-for=\\\"", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "].");

            var ticksThis = DateTime.UtcNow.AddTicks(10).Ticks;
            partial = partial.Replace("name=\\\"" + parentProperty + "[" + ticksParent + "]" + ".", "name=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + collectionProperty + "[" + ticksThis + "].");
            partial = partial.Replace("data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]" + ".", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + collectionProperty + "[" + ticksThis + "].");
            var js = string.Format("javascript:addNestedFormGary(this,'{0}','{1}','{2}','{3}','{4}','{5}');return false;", parentContainerElement, containerElement, counterElement, ticksParent, ticksThis, partial);

            TagBuilder tb = new TagBuilder("a");

            tb.Attributes.Add("href", "#");

            tb.Attributes.Add("onclick", js);

            tb.InnerHtml = linkText;

            var tag = tb.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(tag);
        }

        //public static MvcHtmlString RepalceRealModelToAddNestedForm(this HtmlHelper helper, MvcHtmlString initModelContext, string linkText, string parentContainerElement, string containerElement, string counterElement, string parentProperty, string collectionProperty)
        //{
        //    var ticksParent = DateTime.UtcNow.Ticks;
        //    var partial = initModelContext.ToHtmlString().JsEncode();
        //    partial = partial.Replace("name=\\\"", "name=\\\"" + parentProperty + "[" + ticksParent + "].");
        //    partial = partial.Replace("data-valmsg-for=\\\"", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "].");

        //    var ticksThis = DateTime.UtcNow.AddTicks(10).Ticks;
        //    partial = partial.Replace("name=\\\"" + parentProperty + "[" + ticksParent + "]" + ".", "name=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + collectionProperty + "[" + ticksThis + "].");
        //    partial = partial.Replace("data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]" + ".", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + collectionProperty + "[" + ticksThis + "].");
        //    var js = string.Format("javascript:addNestedFormGary(this,'{0}','{1}','{2}','{3}','{4}','{5}');return false;", parentContainerElement, containerElement, counterElement, ticksParent, ticksThis, partial);

        //    TagBuilder tb = new TagBuilder("a");

        //    tb.Attributes.Add("href", "#");

        //    tb.Attributes.Add("onclick", js);

        //    tb.InnerHtml = linkText;

        //    var tag = tb.ToString(TagRenderMode.Normal);

        //    return MvcHtmlString.Create(tag);
        //}

        public static IHtmlString LinkToAddNestedFormGary<TModel>(this HtmlHelper<TModel> htmlHelper, string linkText, string parentContainerElement,string containerElement, string counterElement,string parentProperty, string collectionProperty, Type nestedType)
        {

            var ticksParent = DateTime.UtcNow.Ticks;

            var nestedObject = Activator.CreateInstance(nestedType);

            var partial = htmlHelper.EditorFor(x => nestedObject).ToHtmlString().JsEncode();

            partial = partial.Replace("id=\\\"nestedObject", "id=\\\"" + parentProperty + "_" + ticksParent + "_");

            partial = partial.Replace("name=\\\"nestedObject", "name=\\\"" + parentProperty + "[" + ticksParent + "]");

            partial = partial.Replace("data-valmsg-for=\\\"nestedObject", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]");

            //partial = partial.Replace("id=\\\"nestedObject", "id=\\\"" + parentProperty + "_" + ticksParent);

            //partial = partial.Replace("name=\\\"nestedObject", "name=\\\"" + parentProperty + "[" + ticksParent + "]");

            //partial = partial.Replace("data-valmsg-for=\\\"nestedObject", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]");

            var ticksThis = DateTime.UtcNow.Ticks;

            partial = partial.Replace("id=\\\"" + parentProperty + "_" + ticksParent + "__" + "nestedObject"+"_", "id=\\\"" + parentProperty + "_" + ticksParent +"_"+ collectionProperty + "_" + ticksThis);

            partial = partial.Replace("name=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + "nestedObject", "name=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + collectionProperty + "[" + ticksThis + "]");

            partial = partial.Replace("data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + "nestedObject", "data-valmsg-for=\\\"" + parentProperty + "[" + ticksParent + "]" + "." + collectionProperty + "[" + ticksThis + "]");

            var js = string.Format("javascript:addNestedFormGary(this,'{0}','{1}','{2}','{3}','{4}','{5}');return false;", parentContainerElement, containerElement, counterElement, ticksParent, ticksThis, partial);

            TagBuilder tb = new TagBuilder("a");

            tb.Attributes.Add("href", "#");

            tb.Attributes.Add("onclick", js);

            tb.InnerHtml = linkText;

            var tag = tb.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(tag);

        }

        public static IHtmlString ReplaceNoParentPartial(this HtmlHelper htmlHelper, MvcHtmlString partial, string containerElement, int thisOrder)
        {
            var content = partial.ToHtmlString();
            content = content.Replace("name=\"", "name=\"" + containerElement + "[" + thisOrder + "].");
            content = content.Replace("data-valmsg-for=\"", "data-valmsg-for=\"" + containerElement + "[" + thisOrder + "].");
            content = content.Replace(containerElement + "[" + thisOrder + "].*", "");
            return MvcHtmlString.Create(content);
        }

        public static IHtmlString ReplaceTestItemConfigEditPartial(this HtmlHelper htmlHelper, MvcHtmlString partial, string containerElement, int thisOrder) 
        {
            var content = partial.ToHtmlString();
            content = content.Replace("name=\"","name=\""+containerElement+"["+thisOrder+"].");
            content = content.Replace("data-valmsg-for=\"", "data-valmsg-for=\""+containerElement+"["+thisOrder+"].");
            content = content.Replace(containerElement+"["+thisOrder+"].*","");
            return MvcHtmlString.Create(content);
        }

        public static IHtmlString ReplacePerConfigEditPartail(this HtmlHelper htmlHelper, MvcHtmlString partial, string parentContainerElement, string containerElment,int parentOrder, int thisOrder,string tick = "") 
        {
            var content = partial.ToHtmlString();
            content = content.Replace(tick,"");
            content = content.Replace("name=\"", "name=\"*" + parentContainerElement + "[" + parentOrder + "]." + containerElment + "[" + thisOrder + "].");
            content = content.Replace("data-valmsg-for=\"", "data-valmsg-for=\"*" + parentContainerElement + "[" + parentOrder + "]." + containerElment + "[" + thisOrder + "].");
            return MvcHtmlString.Create(content);
        }

        public static IHtmlString LinkToAddNestedForm<TModel>(this HtmlHelper<TModel> htmlHelper, string linkText, string containerElement, string counterElement, string collectionProperty, Type nestedType)
        {
            var ticks = DateTime.UtcNow.Ticks;

            var nestedObject = Activator.CreateInstance(nestedType);
            
            var partial = htmlHelper.EditorFor(x => nestedObject).ToHtmlString().JsEncode();

            partial = partial.Replace("id=\\\"nestedObject", "id=\\\"" + collectionProperty + "_" + ticks + "_");

            partial = partial.Replace("name=\\\"nestedObject", "name=\\\"" + collectionProperty + "[" + ticks + "]");

            partial = partial.Replace("data-valmsg-for=\\\"nestedObject", "data-valmsg-for=\\\"" + collectionProperty + "[" + ticks + "]");

            var js = string.Format("javascript:addNestedForm('{0}','{1}','{2}','{3}');return false;", containerElement, counterElement, ticks, partial);

            TagBuilder tb = new TagBuilder("a");

            tb.Attributes.Add("href", "#");

            tb.Attributes.Add("onclick", js);

            tb.InnerHtml = linkText;

            var tag = tb.ToString(TagRenderMode.Normal);

            return MvcHtmlString.Create(tag);

        }

        private static string JsEncode(this string s)
        {

            if (string.IsNullOrEmpty(s)) return "";

            int i;

            int len = s.Length;

            StringBuilder sb = new StringBuilder(len + 4);

            string t;



            for (i = 0; i < len; i += 1)
            {

                char c = s[i];

                switch (c)
                {

                    case '>':

                    case '"':

                    case '\\':

                        sb.Append('\\');

                        sb.Append(c);

                        break;

                    case '\b':

                        sb.Append("\\b");

                        break;

                    case '\t':

                        sb.Append("\\t");

                        break;

                    case '\n':

                        //sb.Append("\\n");

                        break;

                    case '\f':

                        sb.Append("\\f");

                        break;

                    case '\r':

                        //sb.Append("\\r");

                        break;

                    default:

                        if (c < ' ')
                        {

                            //t = "000" + Integer.toHexString(c); 

                            string tmp = new string(c, 1);

                            t = "000" + int.Parse(tmp, System.Globalization.NumberStyles.HexNumber);

                            sb.Append("\\u" + t.Substring(t.Length - 4));

                        }

                        else
                        {

                            sb.Append(c);

                        }

                        break;

                }

            }

            return sb.ToString();

        }

        private static Type GetNonNullableModelType(ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;

            Type underlyingType = Nullable.GetUnderlyingType(realModelType);
            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }
            return realModelType;
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = GetNonNullableModelType(metadata);
            IEnumerable<TEnum> values = Enum.GetValues(enumType).Cast<TEnum>();

            IEnumerable<SelectListItem> items =
                values.Select(value => new SelectListItem
                {
                    Text = value.ToString(),
                    Value = value.ToString(),
                    Selected = value.Equals(metadata.Model)
                });

            //if (metadata.IsNullableValueType)
            //{
            //    items = SingleEmptyItem.Concat(items);
            //}
            items = SingleEmptyItem.Concat(items);

            return htmlHelper.DropDownListFor(
                expression,
                items
                );
        }

        private static readonly SelectListItem[] SingleEmptyItem = new[] { new SelectListItem { Text = "", Value = "" } };
        
        //质量放行驻波或回波损耗显示
        public static MvcHtmlString DisplayZbOrHbsh(this HtmlHelper htmlHelper, List<ICollection<VnaTestItemPerRecord>> list, int index)
        {
            if(list.Count() > 0)
            {
                if(list[0].Count() >= index)
                {
                    var perRecord = list[0].ElementAtOrDefault(index-1);
                    if(perRecord.TestitemPerResult)
                    {
                        var tag = new TagBuilder("span");
                        tag.SetInnerText(string.Format("{0:0.##}", perRecord.XValue/1000000) + "/" + string.Format("{0:0.##}", perRecord.YValue));
                        return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
                    }
                    else
                    {
                        return MvcHtmlString.Empty;
                    }
                }
                else
                {
                    return MvcHtmlString.Empty;
                }
            }
            else
            {
                return MvcHtmlString.Empty;
            } 
        }
        //质量放行时域阻抗或TDR电长度显示
        public static MvcHtmlString DisplaySyzkOrTdr(this HtmlHelper htmlHelper, List<ICollection<VnaTestItemPerRecord>> list, int index)
        {
            if (list.Count() > 0)
            {
                if (list[0].Count() >= index)
                {
                    var perRecord = list[0].ElementAtOrDefault(index - 1);
                    if (perRecord.TestitemPerResult)
                    {
                        var tag = new TagBuilder("span");
                        tag.SetInnerText(string.Format("{0:0.##}", perRecord.RValue));
                        return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
                    }
                    else
                    {
                        return MvcHtmlString.Empty;
                    }
                }
                else
                {
                    return MvcHtmlString.Empty;
                }
            }
            else
            {
                return MvcHtmlString.Empty;
            }
        }
        //质量放行衰减显示
        public static HtmlString DisplaySj(this HtmlHelper htmlHelper, List<VnaTestItemRecord> list)
        {
            if (list.Count() > 0)
            {
                var record = list[0];
                if (record.TestItemResult)
                {
                    var tag = new TagBuilder("span");
                    tag.SetInnerText("不合格");
                    return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
                }
                else
                {
                    return MvcHtmlString.Empty;
                }
            }
            else
            {
                return MvcHtmlString.Empty;
            }
        }
        //质量放行外观显示
        public static HtmlString DisplayWg(this HtmlHelper htmlHelper, List<TestItem> list, VnaRecord vnaRecord)
        {
            if (list.Count() > 0)
            {
                var tag = new TagBuilder("span");
                string names = null;
                foreach (var item in list)
                {
                    if (item.Name == "备注")
                    {
                        names += vnaRecord.Remark + " ";
                    }
                    else 
                    {
                        names += item.Name + " ";
                    }
                }

                tag.SetInnerText(names);
                return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
            }
            else
            {
                return MvcHtmlString.Empty;
            }
        }
        //vna测试非电器性能显示
        public static HtmlString DisplayNotElec(this HtmlHelper htmlHelper, VnaRecord vnaRecord, string notElecName = null)
        {
            if (vnaRecord.VnaTestItemRecords.Where(a => a.TestItem.Name == notElecName).Count() != 0)
            {
                var tag = new TagBuilder("span");
                tag.SetInnerText("√");
                return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
            }
            else
            {
                return MvcHtmlString.Empty;
            }
        }
        //筛选下拉列表
        public static MvcHtmlString FilterCombox(this HtmlHelper htmlHelper, MvcHtmlString filterCondition)
        {
            string htmlString = "<div id='inputcombox'><div><input class='inputcombox-input'/>";
            htmlString += filterCondition;
            htmlString += "</div><div class='inputcombox-div' style='border: 1px solid #E2E2E2;'></div></div>";
            return MvcHtmlString.Create(htmlString);
        }
    }

    public static class AuthorizeActionLinkExtention 
    {
        public static MvcHtmlString AuthorizeActionLink(this HtmlHelper helper, string linkText, string actionName, string controllerName)
        {
            if (HasActionPermission(helper, actionName, controllerName))
                return helper.ActionLink(linkText, actionName, controllerName);

            return MvcHtmlString.Empty;
        }

        static bool HasActionPermission(this HtmlHelper htmlHelper, string actionName, string controllerName = null)
        {
            controllerName = string.IsNullOrEmpty(controllerName) ? htmlHelper.ViewContext.Controller.GetType().Name : controllerName;
            if (controllerName.IndexOf("Controller") > 0)
            {
                controllerName = controllerName.Substring(0, controllerName.IndexOf("Controller"));
            }
            string controllerActionName = controllerName + "_" + actionName;
            var item = HttpContext.Current.Session["PermissionList"];
            return (((List<string>)HttpContext.Current.Session["PermissionList"]).Contains(controllerActionName));
        }
    }

    //LabelExtensions
    public static class LabelExtensions
    {
        public static MvcHtmlString Label(this HtmlHelper html, string expression, string id = "", bool generatedId = false)
        {
            return LabelHelper(html, ModelMetadata.FromStringExpression(expression, html.ViewData), expression, id, generatedId);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString PPLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string id = "", bool generatedId = false)
        {
            return LabelHelper(html, ModelMetadata.FromLambdaExpression(expression, html.ViewData), ExpressionHelper.GetExpressionText(expression), id, generatedId);
        }

        internal static MvcHtmlString LabelHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string id, bool generatedId)
        {
            string labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }
            var sb = new StringBuilder();
            sb.Append(labelText);
            if (metadata.IsRequired)
            {
                sb.Append("*");
            }

            var tag = new TagBuilder("label");
            if (!string.IsNullOrWhiteSpace(id))
            {
                tag.Attributes.Add("id", id);
            }
            else if (generatedId)
            {
                tag.Attributes.Add("id", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName) + "_Label");
            }

            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.SetInnerText(sb.ToString());

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }
    }

    
}