# ContractHttp
A .Net library for contract based Http clients and controllers


### Example Client

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

Once the interface is defined a proxy can be generated and the used to call the service:

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