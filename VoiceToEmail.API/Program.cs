using Microsoft.OpenApi.Models;
using VoiceToEmail.API.Services;
using VoiceToEmail.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddXmlSerializerFormatters();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VoiceToEmail API", Version = "v1" });
});

// Configure HttpClient for Twilio with a named client
builder.Services.AddHttpClient("TwilioClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(2); // Increased timeout for media downloads
});

// Register services
builder.Services.AddHttpClient();
builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWhatsAppService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var transcriptionService = sp.GetRequiredService<ITranscriptionService>();
    var contentService = sp.GetRequiredService<IContentService>();
    var emailService = sp.GetRequiredService<IEmailService>();
    var logger = sp.GetRequiredService<ILogger<WhatsAppService>>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("TwilioClient");

    return new WhatsAppService(
        config,
        transcriptionService,
        contentService,
        emailService,
        httpClient,
        logger
    );
});

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoiceToEmail API V1");
    });
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll");
}

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Log application startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started. Environment: {Environment}", 
    app.Environment.EnvironmentName);

app.Run();