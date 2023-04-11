// <copyright file="Program.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using HearYe.Server;
using HearYe.Server.Helpers;
using HearYe.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging; // HttpLoggingFields
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI; // SubmitMethod

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

/* It's possible to use this code to check claims. Use with the Authorize attribute
 * e.g. [Authorize(Policy = "RequireDbIdClaim")]. I'm not doing this because I often
 * need to use the claim value within the method, and I'd rather not repeat this
 * check.

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireDbIdClaim", policy =>
        policy.RequireAssertion(context => context.User.HasClaim(c =>
            c.Type.Equals("extension_DatabaseId"))));
});*/

builder.Services.Configure<JwtBearerOptions>(
    JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters.NameClaimType = "name";
    });

if (builder.Environment.IsProduction())
{
    SqlConnectionStringBuilder scBuilder = new (builder.Configuration.GetConnectionString("PartialConnection") !)
    {
        DataSource = builder.Configuration.GetSection("AzureSQL")["ServerName"] !,
        UserID = builder.Configuration.GetSection("AzureSQL")["AppId"] !,
        Password = builder.Configuration["AzureSQL_AppRegSecret"],
    };

    builder.Services.AddHearYeContext(scBuilder.ConnectionString);
}
else if (builder.Environment.IsDevelopment())
{
    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
    builder.Logging.AddProvider(new DevFileLoggerProvider(filePath));
    builder.Services.AddHearYeContext(builder.Configuration.GetConnectionString("DefaultConnection") !);
}

string[] graphScopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes").Get<List<string>>() !.ToArray();
string graphTenantId = builder.Configuration.GetSection("MicrosoftGraph")["TenantId"] !;
string graphClientId = builder.Configuration.GetSection("MicrosoftGraph")["ClientId"] !;
string graphAppRegSecret = builder.Configuration["Graph_AppRegSecret"] !;
builder.Services.AddGraphClient(graphScopes, graphTenantId, graphClientId, graphAppRegSecret);

// Converting to a Web API;
// builder.Services.AddControllersWithViews()
//    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096; // default is 32k
    options.ResponseBodyLogLimit = 4096; // default is 32k
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["ClientAddress"] !)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new () { Title = "HearYe Service API", Version = "v1" });

    c.AddSecurityDefinition(
        "AuthToken",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Name = HeaderNames.Authorization,
        });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "AuthToken",
                },
            },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<HearYeContext>();

var app = builder.Build();

// API management requires Swagger definitions to always be generated.
app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    app.UseHttpLogging();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "HearYe Service API Version 1");

        c.SupportedSubmitMethods(new[]
        {
            SubmitMethod.Get, SubmitMethod.Post,
            SubmitMethod.Put, SubmitMethod.Patch,
            SubmitMethod.Delete,
        });
    });

    try
    {
        using IServiceScope scope = app.Services.CreateScope();
        HearYeContext db = scope.ServiceProvider.GetRequiredService<HearYeContext>();
        DbInitializer.Initialize(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while creating the DB.");
    }
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Converting to a Web API;
// app.UseBlazorFrameworkFiles();
// app.UseStaticFiles();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
	app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks(path: "/healthcheck");

// app.MapRazorPages();
app.MapControllers();

if (app.Environment.IsProduction())
{
    app.UseSecretHeader(builder.Configuration["APIM_Secret"] !, "APIM_Secret");
}

// app.MapFallbackToFile("index.html");
app.Run();
