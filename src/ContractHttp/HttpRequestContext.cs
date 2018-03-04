namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    internal class HttpRequestContext
    {
        private MethodInfo methodInfo;

        private object[] arguments;

        private string contentType;

        private int responseArg = -1;

        private int dataArg = -1;

        private Type dataArgType;

        private int requestActionArg = -1;

        private int responseActionArg = -1;

        public HttpRequestContext(MethodInfo methodInfo, object[] arguments, string contentType)
        {
            this.methodInfo = methodInfo;
            this.arguments = arguments;
            this.contentType = contentType;
        }

        /// <summary>
        /// Checks the arguments for specific ones.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="contentArg">The index of the content argument, or -1 if one is not found.</param>
        /// <param name="responseArg">The index of the response argument, or -1 if one is not found.</param>
        /// <param name="dataArg">The index of the data argument, or -1 if one is not found.</param>
        /// <param name="dataArgType">The <see cref="Type"/> of the data argment, or null if one is not found.</param>
        /// <returns>A array of argument names.</returns>
        public string[] CheckArgsAndBuildRequest(
            HttpRequestBuilder requestBuilder)
        {
            this.responseArg = -1;
            this.dataArg = -1;
            this.dataArgType = null;
            this.requestActionArg = -1;
            this.responseActionArg = -1;

            var formUrlAttrs = this.methodInfo.GetCustomAttributes<AddFormUrlEncodedPropertyAttribute>();
            if (formUrlAttrs.Any() == true)
            {
                foreach (var attr in formUrlAttrs)
                {
                    requestBuilder.AddFormUrlProperty(attr.Key, attr.Value);
                }
            }

            bool formUrlContent = false;
            ParameterInfo[] parms = this.methodInfo.GetParameters();
            string[] names = new string[parms.Length];

            if (parms != null)
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    names[i] = parms[i].Name;

                    if (parms[i].IsOut == true)
                    {
                        Type parmType = parms[i].ParameterType.GetElementType();
                        if (parmType == typeof(HttpResponseMessage))
                        {
                            responseArg = i;
                        }
                        else
                        {
                            dataArg = i;
                            dataArgType = parmType;
                        }

                        continue;
                    }

                    if (parms[i].ParameterType == typeof(Action<HttpRequestMessage>))
                    {
                        requestActionArg = i;
                        continue;
                    }

                    if (parms[i].ParameterType == typeof(Action<HttpRequestMessage>))
                    {
                        responseActionArg = i;
                        continue;
                    }

                    var attrs = parms[i].GetCustomAttributes();
                    if (attrs != null &&
                        attrs.Any() == true &&
                        this.arguments[i] != null)
                    {
                        if (requestBuilder.IsContentSet == false)
                        {
                            if (this.HasAttribute(attrs, typeof(SendAsContentAttribute), typeof(FromBodyAttribute)) == true)
                            {
                                requestBuilder.SetContent(new StringContent(
                                    JsonConvert.SerializeObject(this.arguments[i]),
                                    Encoding.UTF8,
                                    contentType));
                            }

                            var formUrlAttr = attrs.OfType<SendAsFormUrlAttribute>().FirstOrDefault();
                            if (formUrlAttr != null)
                            {
                                formUrlContent = true;
                                if (typeof(Dictionary<string, string>).IsAssignableFrom(this.arguments[i].GetType()) == true)
                                {
                                    requestBuilder.SetContent(new FormUrlEncodedContent((Dictionary<string, string>)this.arguments[i]));
                                }
                                else if (typeof(Dictionary<string, object>).IsAssignableFrom(this.arguments[i].GetType()) == true)
                                {
                                    var list = ((Dictionary<string, object>)this.arguments[i]).Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString()));
                                    requestBuilder.SetContent(new FormUrlEncodedContent(list));
                                }
                                else if (this.IsModelObject(parms[i].ParameterType) == true)
                                {
                                    var list = new Dictionary<string, string>();
                                    var properties = this.arguments[i].GetType().GetProperties();
                                    foreach (var property in properties)
                                    {
                                        list.Add(property.Name, property.GetValue(this.arguments[i])?.ToString());
                                    }

                                    requestBuilder.SetContent(new FormUrlEncodedContent(list));
                                }
                                else
                                {
                                    requestBuilder.AddFormUrlProperty(formUrlAttr.Name ?? parms[i].Name, this.arguments[i].ToString());
                                }
                            }
                        }

                        foreach (var query in attrs.OfType<SendAsQueryAttribute>().Select(a => a.Name))
                        {
                            requestBuilder.AddQueryString(query, this.arguments[i].ToString());
                        }

                        foreach (var attr in attrs.OfType<SendAsHeaderAttribute>())
                        {
                            if (string.IsNullOrEmpty(attr.Format) == false)
                            {
                                requestBuilder.AddHeader(attr.Name, string.Format(attr.Format, this.arguments[i].ToString()));
                            }
                            else
                            {
                                requestBuilder.AddHeader(attr.Name, this.arguments[i].ToString());
                            }
                        }
                    }

                    if (formUrlContent == false &&
                        requestBuilder.IsContentSet == false &&
                        this.IsModelObject(parms[i].ParameterType) == true)
                    {
                        requestBuilder.SetContent(new StringContent(
                            JsonConvert.SerializeObject(this.arguments[i]),
                            Encoding.UTF8,
                            contentType));
                    }
                }
            }

            return names;
        }

        private bool IsModelObject(Type type)
        {
            return type.IsPrimitive == false &&
                type.IsClass == true &&
                type != typeof(string);
        }

        /// <summary>
        /// Checks if a list of attributes contains any of a provided list.
        /// </summary>
        /// <param name="attrs">The attribute listto check.</param>
        /// <param name="attrTypes">The attributes to check for.</param>
        /// <returns>True if any are found; otherwise false.</returns>
        private bool HasAttribute(IEnumerable<Attribute> attrs, params Type[] attrTypes)
        {
            foreach (var attr in attrs)
            {
                if (attrTypes.FirstOrDefault(t => t == attr.GetType()) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="inArgs">The in arguments.</param>
        /// <param name="responseArg">The response argument index.</param>
        /// <param name="dataArg">The data argument index.</param>
        /// <param name="dataArgType">The data argument type.</param>
        /// <returns>The result.</returns>
        public object ProcessResult(
            HttpResponseMessage response,
            Type returnType)
        {
            object result = null;
Console.WriteLine("Return Type: {0}", returnType);
            string content = response.Content.ReadAsStringAsync().Result;
            if (content.IsNullOrEmpty() == false)
            {
                if (returnType != typeof(HttpResponseMessage) &&
                    returnType != typeof(void))
                {
                    var fromJsonAttr = this.methodInfo.ReturnParameter.GetCustomAttribute<FromJsonAttribute>();
                    if (fromJsonAttr != null)
                    {
                        result = fromJsonAttr.JsonToObject(content, returnType);
                    }
                    else
                    {
                        result = JsonConvert.DeserializeObject(content, returnType);
                    }
                }
                else if (dataArg != -1)
                {
                    var dataFromJsonAttr = this.methodInfo.GetParameters()[this.dataArg].GetCustomAttribute<FromJsonAttribute>();
                    if (dataFromJsonAttr != null)
                    {
                        this.arguments[dataArg] = dataFromJsonAttr.JsonToObject(content, this.dataArgType);
                    }
                    else
                    {
                        this.arguments[dataArg] = JsonConvert.DeserializeObject(content, this.dataArgType);
                    }
                }
            }

            if (this.responseArg == -1 &&
                returnType != typeof(HttpResponseMessage))
            {
                response.EnsureSuccessStatusCode();
            }

            if (this.responseArg != -1)
            {
                this.arguments[this.responseArg] = response;
            }

            if (returnType == typeof(HttpResponseMessage))
            {
                return response;
            }

            return result;
        }

        public void InvokeRequestAction(HttpRequestMessage request)
        {
            if (this.requestActionArg != -1)
            {
                ((Action<HttpRequestMessage>)this.arguments[requestActionArg])?.Invoke(request);
            }
        }

        public void InvokeResponseAction(HttpResponseMessage response)
        {
            if (this.responseActionArg != -1)
            {
                ((Action<HttpResponseMessage>)this.arguments[responseActionArg])?.Invoke(response);
            }
        }
    }
}