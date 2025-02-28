# ContractHttp

A .Net library for contract based Http clients and controllers

## Installation

[ContractHttp is available on Nuget](https://www.nuget.org/packages/ContractHttp)

## Defining a client

A client is defined by creating an interface and decorating it with the HttpClientContractAttribute class. This allows you to define the base route and content type to be used by default by all methods on the interface. These can be also be set on a per method basis and so do not have to be configured at this point.

A call to a REST endpoint is defined by adding a method to the interface. The method determines the parameters and return type that will be used to send data to and receive data from the REST endpoint.

Certain special parameters are supported to allow interception of the http request and response at points in the call (More on that later).

How the method is translated into the appropriate REST call is determined by the attributes used to decorate the method.

Methods can be defined as synchronous or asynchronous.

### Method Parameters and Return Types

#### Return Types

As mentioned earlier methods can be synchronous or asynchronous so supported return types    are:

* void - No return value.
* T - Any type that the content can be deserialised to.
* HttpResponseMessage - The full HttpResponseMessage.
* Task - A task that upon completion will have no return value.
* Task\<T> - A task that upon completion will return any type the content can be deserailised to.
* Task\<HttpResponseMessage> - A task that upon completion will return the full HttpResponseMethod.
* Task\<Stream> - A task that upon completion will return the content property as a stream.

Return values can also be decorated with attributes to control there behaviour:

FromJsonAttribute can be used to return a property from json response content.

FromModelAttribute can be used to return a property from response content deserialised as a specific model.

#### Parameters

The methods parameters can be used to define content, query parameters, or headers, and attributes are used to determine their use.

* SendAsContentAttribute - Specifies that a parameter is to be used as the requests content.

* SendAsQueryAttribute - Specifies that a parameter is to be used as a query parameter.

* SendAsHeaderAttribute - Specfies that a parameter is to be used as a request header.

* SendAsFormUrlAttribute - Specifies that a paremeter is to be used as a form url content property.

```cs
public interface ICustomerClient
{
    [HttpGet("api/customer/update")]
    Task<HttpResponseMessage> UpdateCustomerAsync([SendAsHeader]string id, [SendAsContent]CustomerModel customer);
}
```

#### Out Parameters

Out parameters are supported for returning data on synchronous methods:

* An out parameter with no attribute that is also not defined a "special" parameter can be used to return data from the response content.

* An out parameter decorated with FromHeaderAttribute can be used to return data from a response header.

* An out parameter decorated with FromModelAttribute can be used to return a property from a model that was deserialised from the requests content.

```cs
public interface ICustomerClient
{
    [HttpGet("api/customer/{id}")]
    CustomerModel GetCustomer(string id, [FromHeader(Name = "X-Account")] out string account);
}
```

#### Special Parameters

Special parameter types are also supported:

* A parameter of type Action\<HttpRequestMessage> will be called just before the request is sent.

* A parameter of type Action\<HttpResponseMessage> will be called just after the response has been received.

* A parameter of type Func\<HttpResponseMessage, *ReturnType*> will be called just before the method returns. The method will then return the response from the lambda allowing it to decode the response.

```cs
public interface ICustomerClient
{
    [HttpGet("api/customer/{id}")]
    Task<CustomerModel> GetCustomerAsync(string id, Action<HttpRequestMessage> requestAction, Func<HttpResponseMessage, CustomerMode> responseFunc);
}
```

### Attributes

Attributes are used to define a methods behaviours:

The http method attributes from Microsoft.AspNetCore.Mvc such as [HttpGet], [HttpPut], [HttpPost], and [HttpDelete] can be used to define the requests HttpMethod type.

ContractHttp also provides its own set of attributes as well:

* PostAttribute - Specifies that the request has a POST method.
* GetAttribute - Specifies that the request has a GET method.
* PutAttribute - Specifies that the request has a PUT method.
* PatchAttribute - Specifies that the request has a PATCH method.
* DeleteAttribute - Specifies that the request has a DELETE method.

AddHeaderAttribute can be applied to a method or an interface to add a header value to the request or all requests:

* AddFormUrlEncodedPropertyAttribute can be used to add a form url property to a request content.

* AddAuthorizationHeaderAttribute can be applied to a method or an interface to add an authorization header to the request or all requests.

One group of attributes can be used to alter how return values or out parameters use the Http responses content to obtain their values.

* The **FromResponseAttribute** class can be used to indicate the return type. This is only required if the methods return type or out parameter is an interface type.

* The **FromJsonAttribute** class can be used to indicate that the value is obtained from a JSON object. It can be used to specify a sub type within the JSON object via a path parameter. It is also possible to indicate a return type (as with FromResponseAttribute) if the methods return type is an interface.

* The **FromModelAttribute** class can be used to indicate that the value is obtained from a property of a model, rather than the model itself.

### Special Properties

When defining the return type it is possible to use 'special properties' to obtain extra information from the response. If the return type contains one of the special properties, then
the corresponding response values are put into those properties when the call returns.

* Any property of type **HttpResponseMessage** will return the http response.

* Any property of type **HttpStatusCode** will return the status code.

```cs
public class CustomerModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public HttpResponseMessage Response { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
```

```cs
public interface ICustomerClient
{
    [HttpGet("{id}")]
    Task<CustomerModel> GetCustomerAsync(string id);
}
```

### Http Response Processors

For clients that wish to create a more complex response model there is the `HttpResponseProcessor` type.
These types allow the response to be intercepted and converted into much more complex results.

```cs
public interface IResult<T>
{
    int StatusCode { get; }
    T Result { get; }
    string ErrorMessage { get; }
}
```

```cs
public class Result<T>
    : IResult<T>
{
    public int StatusCode { get; set; }
    public T Result { get; set; }
    public string ErrorMessage { get; set; }
}
```

```cs
public class ResultProcessor<T>
    : HttpResponseProcessor<IResult<T>>
{
    public override async Task<IResult<T>> ProcessResponseAsync(HttpResponseMessage httpResponse)
    {
        var result = new Result<T>;
        result.StatusCode = (int)httpResponse.StatusCode;
        if (httpResponse.IsSuccessStatusCode == true)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content) == false)
            {
                result.Result = JsonConvert.DeserializeObject<T>(content);
            }
        }
        else
        {
            result.ErrorMessage = "";
        }

        return result;
    }
}
```

```cs
[HttpClientContract(Route = "api/customers", ContentType = "aplication/json")]
public interface ICustomerClient
{
    [HttpGet("")]
    [HttpResponseProcessor(typeof(ResultProcessor<IEnumerable<CustomerModel>>))]
    Task<IResult<IEnumerable<CustomerModel>>> GetCustomersAsync();
}
```

### Example synchronous client

Create an interface and decorate with attributes to define how the client should interact with the service:

```cs
[HttpClientContract(Route = "api/customers", ContentType = "aplication/json")]
public interface ICustomerClient
{
    [HttpPost("")]
    CreateCustomerResponseModel CreateCustomer(CreateCustomerModel customer);

    [HttpGet("")]
    IEnumerable<CustomerModel> GetCustomers();

    [HttpGet("{name}")]
    CustomerModel GetCustomerByName(string name);
}
```

Once the interface is defined a proxy can be generated and then used to call the service:

```cs
var httpClient = new HttpClient();
var httpProxy = new HttpClientProxy<ICustomerClient>(
    "http://localhost",
    httpClient);

var customerClient = httpProxy.GetProxyObject();

var response = customerClient.CreateCustomer(
    new CreateCustomerModel()
    {
        Name = "Customer",
        Address = "Somewhere",
        PhoneNumber = "123456789"
    });
```

### Example asynchronous client

Create an interface and decorate with attributes to define how the client should interact with the service:

```cs
[HttpClientContract(Route = "api/customers", ContentType = "aplication/json")]
public interface ICustomerClient
{
    [HttpPost("")]
    Task<CreateCustomerResponseModel> CreateCustomerAsync(CreateCustomerModel customer);

    [HttpGet("")]
    Task<IEnumerable<CustomerModel>> GetCustomersAsync();

    [HttpGet("{name}")]
    Task<CustomerModel> GetCustomerByName(string name);
}
```

Again, once the interface is defined a proxy can be generated and then used to call the service:

```cs
var httpClient = new HttpClient();
var httpProxy = new HttpClientProxy<ICustomerClient>(
    "http://localhost",
    httpClient);

var customerClient = httpProxy.GetProxyObject();

var response = await customerClient.CreateCustomerAsync(
    new CreateCustomerModel()
    {
        Name = "Customer",
        Address = "Somewhere",
        PhoneNumber = "123456789"
    });
```

## Services

### Example Service

Create an interface and decorate with attributes to define the controllers behaviour:

```cs
[HttpController(RoutePrefix = "api/customers")]
public interface ICustomerService
{
    [HttpGet("")]
    IEnumerable<CustomerModel> GetAll();

    [HttpGet("{name}")]
    CustomerModel GetByName(string name);

    [HttpPost("")]
    CreateCustomerResponseModel CreateCustomer(CreateCustomerModel model);

    [HttpDelete("{name}")]
    void Delete(string name);
}
```

Create a Startup.cs class:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc()
            .AddDynamicController<ICustomerService>(
                new CustomerServiceImplementation());
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
}
```

Create a Web host:

```cs
var webHost = new WebHostBuilder()
    .UseKestrel()
    .UseStartup<Startup>()
    .UseUrls("http://localhost")
    .Build();

webHost.Run();
```
