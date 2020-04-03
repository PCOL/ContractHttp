namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection;
    using FluentIL;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller emit extension methods.
    /// </summary>
    public static class ControllerEmitExtensionMethods
    {
        /// <summary>
        /// The status code method.
        /// </summary>
        private static readonly MethodInfo StatusCodeMethod = typeof(ControllerBase).GetMethod("StatusCode", new Type[] { typeof(int) });

        /// <summary>
        /// The status code method with result.
        /// </summary>
        private static readonly MethodInfo StatusCodeWithResultMethod = typeof(ControllerBase).GetMethod("StatusCode", new Type[] { typeof(int), typeof(object) });

        /// <summary>
        /// Emits a call to return a status code as an <see cref="IActionResult"/>.
        /// </summary>
        /// <param name="emitter">A IL Generator.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="localResponse">A local to receive the <see cref="IActionResult"/>.</param>
        /// <returns>The emitter instance.</returns>
        public static IEmitter EmitStatusCodeCall(this IEmitter emitter, int statusCode, ILocal localResponse)
        {
            return emitter
                .LdArg0()
                .LdcI4(statusCode)
                .CallVirt(StatusCodeMethod)
                .StLocS(localResponse);
        }

        /// <summary>
        /// Emits a call to the return a status code and result as an <see cref="IActionResult"/>.
        /// </summary>
        /// <param name="emitter">An emitter.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="local">A local containg the result.</param>
        /// <param name="localResponse">A local to store the response in.</param>
        /// <returns>The emitter instance.</returns>
        public static IEmitter EmitStatusCodeCall(
            this IEmitter emitter,
            int statusCode,
            ILocal local,
            ILocal localResponse)
        {
            emitter
                .LdArg0()
                .LdcI4(statusCode);

            if (local != null)
            {
                emitter
                    .LdLocS(local);
            }
            else
            {
                emitter
                    .LdNull();
            }

            return emitter
                .CallVirt(StatusCodeWithResultMethod)
                .StLocS(localResponse);
        }

        /// <summary>
        /// Resolves any Mvc attributes on the method.
        /// </summary>
        /// <param name="methodBuilder">The method being built.</param>
        /// <param name="methodInfo">The method being called.</param>
        /// <returns>True if any resolved; otherwise false.</returns>
        public static bool ResolveMvcAttributes(
            this IMethodBuilder methodBuilder,
            MethodInfo methodInfo)
        {
            HttpGetAttribute getAttr = methodInfo.GetCustomAttribute<HttpGetAttribute>(true);
            if (getAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpGetAttribute>(getAttr.Template));
                return true;
            }

            HttpPostAttribute postAttr = methodInfo.GetCustomAttribute<HttpPostAttribute>(true);
            if (postAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPostAttribute>(postAttr.Template));
                return true;
            }

            HttpPutAttribute putAttr = methodInfo.GetCustomAttribute<HttpPutAttribute>(true);
            if (putAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPutAttribute>(putAttr.Template));
                return true;
            }

            HttpPatchAttribute patchAttr = methodInfo.GetCustomAttribute<HttpPatchAttribute>(true);
            if (patchAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPatchAttribute>(patchAttr.Template));
                return true;
            }

            HttpDeleteAttribute deleteAttr = methodInfo.GetCustomAttribute<HttpDeleteAttribute>(true);
            if (deleteAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpDeleteAttribute>(deleteAttr.Template));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resolves any <see cref="HttpControllerEndPointAttribute"/> attributes on the controller method.
        /// </summary>
        /// <param name="methodBuilder">A <see cref="IMethodBuilder"/> instance.</param>
        /// <param name="methodInfo">A method info.</param>
        /// <returns>The <see cref="IMethodBuilder"/> instance.</returns>
        public static IMethodBuilder ResolveHttpControllerEndPointAttribute(
            this IMethodBuilder methodBuilder,
            MethodInfo methodInfo)
        {
            HttpControllerEndPointAttribute attr = methodInfo?.GetCustomAttribute<HttpControllerEndPointAttribute>(false);
            if (attr != null)
            {
                if (attr.Method == HttpCallMethod.HttpGet)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpGetAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpPost)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPostAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpPut)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPutAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpPatch)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPatchAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpDelete)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpDeleteAttribute>(attr.Route));
                }
            }

            return methodBuilder;
        }
    }
}