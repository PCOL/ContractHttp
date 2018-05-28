namespace ContractHttp
{
    /// <summary>
    /// Method argument types.
    /// </summary>
    internal enum MethodArgType
    {
        /// <summary>
        /// A data argument.
        /// </summary>
        DataArgument,

        /// <summary>
        /// A response argument.
        /// </summary>
        ResponseArgument,

        /// <summary>
        /// A request action argument.
        /// </summary>
        RequestActionArgument,

        /// <summary>
        /// A response action argument.
        /// </summary>
        ResponseActionArgument,

        /// <summary>
        /// A response function argument.
        /// </summary>
        ResponseFuncArgument,

        /// <summary>
        /// A cancelation token argument.
        /// </summary>
        CancellationTokenArgument,

        /// <summary>
        /// A header argument.
        /// </summary>
        HeaderArgument,

        /// <summary>
        /// A model argument.
        /// </summary>
        ModelArgument,
    }
}