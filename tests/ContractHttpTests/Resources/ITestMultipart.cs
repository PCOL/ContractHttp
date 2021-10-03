namespace ContractHttpTests.Resources
{
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;

    /// <summary>
    /// Interface to test file uploading.
    /// </summary>
    [HttpClientContract(Route = "api/files")]
    public interface ITestMultipart
    {
        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="fileName">The file name to send in the content disposition.</param>
        /// <param name="fileStream">The stream to send.</param>
        /// <returns>A http response.</returns>
        [Upload("upload")]
        Task<HttpResponseMessage> UploadFileAsync(
            [SendAsContentDisposition(IsFileName = true)]
            string fileName,
            [SendAsContent]
            Stream fileStream);

        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="fileName">The file name to send in the content disposition.</param>
        /// <param name="fileStream">The stream to send.</param>
        /// <returns>A http response.</returns>
        [Upload("upload")]
        HttpResponseMessage UploadFile(
            [SendAsContentDisposition(IsFileName = true)]
            string fileName,
            [SendAsContent]
            Stream fileStream);
    }
}