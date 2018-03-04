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

    /// <summary>
    /// Represents a request call context.
    /// </summary>
    internal class HttpRequestContext
    {
        /// <summary>
        /// A reference to the method.
        /// </summary>
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// A reference to the methods arguments.
        /// </summary>
        private readonly object[] arguments;

        /// <summary>
        /// A reference to the requests content type.
        /// </summary>
        private readonly string contentType;

        /// <summary>
        /// The index of the response argument.
        /// </summary>
        private int responseArg = -1;

        /// <summary>
        /// The index of the data argument.
        /// </summary>
        private int dataArg = -1;

        /// <summary>
        /// The data argument type.
        /// </summary>
        private Type dataArgType;

        /// <summary>
        /// The request action.
        /// </summary>
        private Action<HttpRequestMessage> requestAction;

        /// <summary>
        /// The response action.
        /// </summary>
        private Action<HttpResponseMessage> responseAction;

        /// <summary>
        /// Initialises a new instance of the <see cref="HttpRequestContext"/> class.
        /// </summary>
        /// <param name="methodInfo">The methods <see cref="MethodInfo"/>.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <param name="contentType">The content type.</param>
        public HttpRequestContext(MethodInfo methodInfo, object[] arguments, string contentType)
        {
            this.methodInfo = methodInfo;
            this.arguments = arguments;
            this.contentType = contentType;
        }

        /// <summary>
        /// Checks the arguments for specific ones.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <returns>An array of argument names.</returns>
        public string[] CheckArgsAndBuildRequest(
            HttpRequestBuilder requestBuilder)
        {
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
                        this.requestAction = (Action<HttpRequestMessage>)this.arguments[i];
                        continue;
                    }

                    if (parms[i].ParameterType == typeof(Action<HttpRequestMessage>))
                    {
                        responseAction = (Action<HttpResponseMessage>)this.arguments[i];
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

        /// <summary>
        /// Checks if the type is a model object.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>True if the type is a model object; otherwise false.</returns>
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
        /// <returns>The result.</returns>
        public object ProcessResult(
            HttpResponseMessage response,
            Type returnType)
        {
            object result = null;
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

                if (this.dataArg != -1)
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

        /// <summary>
        /// Invokes the request action if it has been defined.
        /// </summary>
        /// <param name="request">A <see cref="HttpRequestMessage"/>.</param>
        public void InvokeRequestAction(HttpRequestMessage request)
        {
            this.requestAction?.Invoke(request);
        }

        /// <summary>
        /// Invokes the response action if it has been defined.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        public void InvokeResponseAction(HttpResponseMessage response)
        {
            this.responseAction?.Invoke(response);
        }
    }
}