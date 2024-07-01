using hng11_stage1;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHttpClient<IpApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/api/hello", async ([FromQuery] string visitor_name, HttpContext context, IpApiClient ipApiClient) =>
{
    var ipAddress = context.GetServerVariable("HTTP_X_FORWARDED_FOR") ?? context.Connection.RemoteIpAddress?.ToString();
    var ipAddressWithoutPort = ipAddress?.Split(':')[0];
    var ipApiResponse = await ipApiClient.Get(ipAddressWithoutPort, CancellationToken.None);
    return Results.Ok(new
    {
        client_ip=ipApiResponse.query,
        location=ipApiResponse.city,
        greeting =$"Hello, {visitor_name}!, the temperature is 11 degrees Celsius in ${ipApiResponse.city}"
    });
})
.WithName("greetings")
.WithOpenApi();

app.Run();