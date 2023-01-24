using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpLogging; // HttpLoggingFields
using Microsoft.Net.Http.Headers;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerUI; // SubmitMethod
using HearYe.Shared;
using HearYe.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.Configure<JwtBearerOptions>(
    JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters.NameClaimType = "name";
    });

builder.Services.AddHearYeContext(builder.Configuration.GetConnectionString("DefaultConnection")!);

string[] graphScopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes").Get<List<string>>()!.ToArray();
string graphTenantId = builder.Configuration.GetSection("MicrosoftGraph")["TenantId"]!;
string graphClientId = builder.Configuration.GetSection("MicrosoftGraph")["ClientId"]!;
string graphAppRegSecret = builder.Configuration["Graph:AppRegSecret"]!;
builder.Services.AddGraphClient(graphScopes, graphTenantId, graphClientId, graphAppRegSecret);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddRazorPages();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestBodyLogLimit = 4096; // default is  32k
    options.ResponseBodyLogLimit= 4096; // default is  32k
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "HearYe Service API",
        Version = "v1"
    });

    c.AddSecurityDefinition("AuthToken",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
            In = ParameterLocation.Header,
            Name = HeaderNames.Authorization
        });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "AuthToken"
                    },
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.AddHealthChecks().AddDbContextCheck<HearYeContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    app.UseHttpLogging();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json",
            "HearYe Service API Version 1");

        c.SupportedSubmitMethods(new[]
        {
            SubmitMethod.Get, SubmitMethod.Post,
            SubmitMethod.Put, SubmitMethod.Patch,
            SubmitMethod.Delete
        });
    });

    try
    {
        using (IServiceScope scope = app.Services.CreateScope())
        {
            HearYeContext db = scope.ServiceProvider.GetRequiredService<HearYeContext>();
            DbInitializer.Initialize(db);
        }
    }
    catch (Exception ex) 
    {
        app.Logger.LogError(ex, "An error occurred while creating the DB.");
    }
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks(path: "/healthcheck");

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
