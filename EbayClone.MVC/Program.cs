using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<EbayClone.MVC.Filters.AdminApiExceptionFilter>());
builder.Services.AddSignalR();
builder.Services.AddScoped<EbayClone.MVC.Services.AdminNotificationService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<EbayClone.MVC.Services.IApiCache, EbayClone.MVC.Services.ApiCacheService>();
builder.Services.AddHttpClient<EbayClone.MVC.Services.CacheRefreshApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<EbayClone.MVC.Services.CacheRefreshBackgroundService>();
builder.Services.AddHttpClient("api-health", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddHttpClient<EbayClone.MVC.Services.AdminApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler((serviceProvider, request) =>
{
    if (request.Method != HttpMethod.Get)
        return Polly.Policy.NoOpAsync<HttpResponseMessage>();

    var logger = serviceProvider.GetRequiredService<ILogger<EbayClone.MVC.Services.AdminApiClient>>();
    return Polly.Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(response => response.StatusCode is
            System.Net.HttpStatusCode.RequestTimeout or
            System.Net.HttpStatusCode.TooManyRequests or
            System.Net.HttpStatusCode.InternalServerError or
            System.Net.HttpStatusCode.BadGateway or
            System.Net.HttpStatusCode.ServiceUnavailable or
            System.Net.HttpStatusCode.GatewayTimeout)
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromMilliseconds(500 * Math.Pow(2, retryAttempt - 1)),
            (outcome, delay, retryAttempt, _) =>
            {
                var result = outcome.Result is null
                    ? outcome.Exception?.GetType().Name
                    : ((int)outcome.Result.StatusCode).ToString();
                logger.LogWarning(
                    "API retry {RetryAttempt}/3 for {Method} {Path} after {Delay}ms. Result: {Result}",
                    retryAttempt,
                    request.Method,
                    request.RequestUri?.PathAndQuery,
                    delay.TotalMilliseconds,
                    result);
            });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapHub<EbayClone.MVC.Hubs.AdminNotificationHub>("/hubs/admin-notifications");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
