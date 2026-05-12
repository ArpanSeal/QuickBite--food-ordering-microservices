using Newtonsoft.Json;
using ProjectSolution.Web.Models;
using ProjectSolution.Web.Service.IService;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static ProjectSolution.Web.Utility.SD;

namespace ProjectSolution.Web.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearerToken)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("ProjectSolution");
            using HttpRequestMessage requestMessage = new();

            /** 1️) Important difference: Content-Type vs Accept
             * 
                Content-Type
                Describes the format of the request body you are sending.

                Example when uploading a file:
                            Content-Type: multipart/form-data

                This is automatically set when you use:
                            var content = new MultipartFormDataContent();
                            You do not need to set it manually.

                Accept
                Describes what response format you want from the server.

                Example:
                        Accept: application/json

                Meaning:
                “I expect the API to return JSON.” **/

            // Set Accept header to indicate we want JSON response. For both JSON requests and file uploads, use:
            requestMessage.Headers.Add("Accept", "application/json");

            /** or,
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); **/


            //token
            if (withBearerToken)
            {
                string? token = _tokenProvider.GetToken();
                //requestMessage.Headers.Add("Authorization", $"Bearer {token}");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            requestMessage.RequestUri = new Uri(requestDto.Url);

            if (requestDto.Data != null)
            {
                if (requestDto.ContentType == ContentType.MultipartFormData)
                {
                    var content = new MultipartFormDataContent(); /** don't use using var:because the request hasn't been sent yet. Let HttpClient handle disposal when the request completes.
                    1️) Create multipart form container
                    var content = new MultipartFormDataContent();
                    What this does

                    Creates an HTTP request body of type:
                    multipart/form-data

                    This format is required when sending:
                    files
                    form fields
                    both together
                    **/

                    foreach (var prop in requestDto.Data.GetType().GetProperties()) // using C# Reflection method: ref c# 1.5
                    {
                        var value = prop.GetValue(requestDto.Data);
                        if (value != null)
                        {
                            if (value is IFormFile file) // check if value implements IFormFile, if true, store it in variable file.
                            {
                                var fileStream = file.OpenReadStream(); // don't use using here for the same reason as mentioned before. So the stream is automatically disposed at the end of the scope.

                                /* 
                                 * When using HttpClient content streams:

                                Object	                            Use using?
                                HttpClient	                        ❌ No
                                MultipartFormDataContent            ❌ No
                                StreamContent	                    ❌ No
                                FileStream used inside HttpContent	❌ No
                                HttpResponseMessage	                ✅ Yes
                                HttpRequestMessage	                ✅ Yes

                                The reason is simple:

                                HttpClient needs the content objects alive while sending the request.
                                 */

                                var fileContent = new StreamContent(fileStream);

                                /* *
                                 5️) Convert file into stream content
                                    var fileContent = new StreamContent(file.OpenReadStream());
                                    Why StreamContent?

                                    We need to send binary file data.

                                    file.OpenReadStream() opens the uploaded file stream.

                                    Example:

                                    photo.jpg

                                    becomes a readable stream of bytes.

                                    StreamContent wraps this stream so HttpClient can send it.

                                    Important benefit:

                                    StreamContent → streaming upload

                                    This avoids loading the entire file into memory.
                                 * */


                                fileContent.Headers.ContentType =
                                    new MediaTypeHeaderValue(file.ContentType);

                                /*
                                 * 6️) Set file content type
                                    fileContent.Headers.ContentType =
                                        new MediaTypeHeaderValue(file.ContentType);

                                    Example:
                                    image/jpeg
                                    image/png

                                    Why this matters:
                                    Set content type for file
                                    Why?
                                    The server needs to know the file type.
                                    file.ContentType provides the MIME type (e.g., image/jpeg).
                                    This ensures the server can process the file correctly.
                                    The receiving API may validate file type.

                                    Example request part:

                                    Content-Type: image/jpeg

                                    Without this header some APIs reject the upload.
                                 */

                                content.Add(fileContent, prop.Name, file.FileName);

                                /*
                                 * 7️) Add file to multipart body
                                    content.Add(fileContent, prop.Name, file.FileName);

                                    Parameters mean:

                                    content.Add(
                                        file data,
                                        field name,
                                        file name
                                    )

                                    Example result:
                                    Content-Disposition: form-data; name="ImageFile"; filename="laptop.jpg"

                                    prop.Name ensures the field name matches the DTO property.
                                    So the backend API can bind it to:
                                    IFormFile ImageFile
                                 
                            Explanation:
                                Adds the file stream to the multipart form data.
                                prop.Name is the form field name(e.g., "Photo").
                                    file.FileName is the original filename(e.g., "photo.jpg").
                                    This creates a multipart section like:
                                    --boundary
                                    Content-Disposition: form-data; name = "Photo"; filename = "photo.jpg"
                                    Content-Type: image / jpeg
                                    [file content bytes]
                                 */
                            }
                            else
                            {
                                content.Add(new StringContent(value?.ToString() ?? string.Empty), prop.Name);

                                /*
                                 * 8️) Handle normal fields
                                    If the property is not a file, it must be added as text.

                                    Example:

                                    Name = "Laptop"
                                    Price = 1500

                                    StringContent converts the value to text.
                                    Here is why StringContent is necessary:
                                    Type Compatibility: The Add method's signature requires the first parameter to be of type HttpContent. StringContent is a specific subclass of HttpContent designed to wrap string data for HTTP requests.
                                    HTTP Headers: Unlike a plain string, StringContent allows the HttpClient to manage essential metadata, such as the Content-Type (which defaults to text/plain for StringContent) and character encoding (like UTF-8).
                                    Serialization: MultipartFormDataContent acts as a container for multiple pieces of "content." By wrapping your string in StringContent, you are telling the system how to serialize that specific part of the multipart form body. 
                                    Microsoft Learn
                                    Microsoft Learn
                                        +5
                                    Direct Comparison:
                                    prop.Name: This is passed as a string because it represents the name of the form field.
                                    new StringContent(...): This is used because it represents the value or "body" of that form field, which must be formatted as HTTP content.
                                 * */
                            }
                        }
                    }
                    requestMessage.Content = content;
                }
                else if (requestDto.ContentType == ContentType.ApplicationJson)
                {
                    string jsonData = JsonConvert.SerializeObject(requestDto.Data);
                    requestMessage.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                }
            }

            switch (requestDto.ApiType)
            {
                case ApiType.POST:
                    requestMessage.Method = HttpMethod.Post;
                    break;
                case ApiType.PUT:
                    requestMessage.Method = HttpMethod.Put;
                    break;
                case ApiType.DELETE:
                    requestMessage.Method = HttpMethod.Delete;
                    break;
                default:
                    requestMessage.Method = HttpMethod.Get;
                    break;
            }

            HttpResponseMessage? responseMessage = null;
            responseMessage = await httpClient.SendAsync(requestMessage);

            try
            {
                string responseBody = await responseMessage.Content.ReadAsStringAsync();
                //Console.WriteLine(responseBody);
                ResponseDto? apiResponseDto = null;
                if (!string.IsNullOrEmpty(responseBody))
                {
                    apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(responseBody);
                }

                // ✅ Return DTO only for 2xx status codes
                if (responseMessage.IsSuccessStatusCode)
                {
                    return apiResponseDto ?? new ResponseDto { IsSuccess = true };
                }

                //🔥 Cleaner Version(Advanced but recommended)
                //return new ResponseDto
                //{
                //    IsSuccess = false,
                //    Message = apiResponseDto?.Message ?? responseMessage.ReasonPhrase ?? string.Empty
                //};

                //long way of handling errors
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new ResponseDto { IsSuccess = false, Message = apiResponseDto?.Message ?? "Not Found" };
                    case HttpStatusCode.BadRequest:
                        return new ResponseDto { IsSuccess = false, Message = apiResponseDto?.Message ?? "Bad Request" };
                    case HttpStatusCode.Forbidden:
                        return new ResponseDto { IsSuccess = false, Message = apiResponseDto?.Message ?? "Access Denied" };
                    case HttpStatusCode.InternalServerError:
                        return new ResponseDto { IsSuccess = false, Message = apiResponseDto?.Message ?? "Internal Server Error" };
                    case HttpStatusCode.Unauthorized:
                        return new ResponseDto { IsSuccess = false, Message = apiResponseDto?.Message ?? "Unauthorized" };
                    default:
                        return new ResponseDto
                        {
                            IsSuccess = false,
                            Message = apiResponseDto?.Message
                                      ?? $"Request failed with status code {(int)responseMessage.StatusCode}"
                        };
                }
            }
            catch (Exception ex)
            {
                // 🚨 Only network, Timeout, DNS failure, runtime exceptions land here
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
                //🧠 Key Idea(Important)
                //✔ HTTP errors ≠ exceptions

                //So:

                //switch handles HTTP status codes(4xx, 5xx)

                //catch handles real exceptions(network, timeout, serialization, etc.)
            }
        }
    }
}
