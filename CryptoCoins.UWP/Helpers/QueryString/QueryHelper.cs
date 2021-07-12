using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CryptoCoins.UWP.Helpers.QueryString
{
    public class QueryHelper
    {
        private static void AppendQuery(StringBuilder url, string key, string value)
        {
            AppendQuery(url, $"{key}={value}");
        }

        private static void AppendQuery(StringBuilder url, string query)
        {
            if (url.Length != 0)
            {
                url.Append("&");
            }
            url.Append(query);
        }

        /// <summary>
        ///     Converts members of an object to query string. Members must have attribute
        ///     <see cref="QueryParameterBaseAttribute" />
        ///     to be included to query string.
        /// </summary>
        /// <param name="requestObject"></param>
        /// <returns></returns>
        public static string QueryString(object requestObject)
        {
            var sb = new StringBuilder();
            foreach (var property in requestObject.GetType().GetProperties())
            {
                var value = property.GetValue(requestObject);
                var queryAttribute = property.GetCustomAttribute<QueryParameterBaseAttribute>(true);
                if (queryAttribute is InlineQueryParameterAttribute)
                {
                    AppendQuery(sb, QueryString(value));
                }
                else if (queryAttribute is QueryParameterListAttribute keyListAttr)
                {
                    if (value is IEnumerable list)
                    {
                        var listSb = new StringBuilder();
                        foreach (var item in list)
                        {
                            listSb.Append(item);
                            listSb.Append(',');
                        }
                        if (listSb.Length > 0)
                        {
                            listSb.Remove(listSb.Length - 1, 1);
                            var query = listSb.ToString();
                            if (query.Length > keyListAttr.MaxLength)
                            {
                                var i = query.LastIndexOf(',', keyListAttr.MaxLength);
                                if (i == -1)
                                {
                                    i = 0;
                                }
                                query = query.Substring(0, i);
                            }
                            AppendQuery(sb, keyListAttr.Key, query);
                        }
                    }
                }
                else if (queryAttribute is QueryParameterAttribute keyAttr)
                {
                    if (value != null)
                    {
                        AppendQuery(sb, keyAttr.Key, value.ToString());
                    }
                }
                else if (queryAttribute is QueryParameterMapAttribute)
                {
                    if (value != null)
                    {
                        var values = (Dictionary<string, IList<string>>) value;
                        foreach (var pair in values)
                        {
                            foreach (var v in pair.Value)
                            {
                                AppendQuery(sb, pair.Key, v);
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}
