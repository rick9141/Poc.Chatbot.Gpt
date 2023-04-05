using Microsoft.OpenApi.Models;
using Poc.Chatbot.Gpt;
using Poc.Chatbot.Gpt.Middleware;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text.Json.Serialization;

const string PROJECT_NAME = "Poc.Chatbot.Gpt";
const string API_VERSION = "v1";
const string XML_EXTENSION = ".xml";
const string SWAGGERFILE_PATH = "./swagger/v1/swagger.json";
const string SECURITY_TYPE = "oauth2";
const string SECURITY_DESCRIPTION = "Consulte a Wiki de ferramentas úteis para obter a chave. Exemplo: \"Key {apiKey}\"";
const string SECURITY_HEADER_NAME = "Authorization";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingletons(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddSwaggerGenNewtonsoftSupport();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(API_VERSION, new OpenApiInfo
    {
        Title = PROJECT_NAME,
        Version = API_VERSION
    });

    var xmlFile = Assembly.GetExecutingAssembly().GetName().Name + XML_EXTENSION;
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("X-API-KEY", new OpenApiSecurityScheme
    {
        Description = SECURITY_DESCRIPTION,
        In = ParameterLocation.Header,
        Name = SECURITY_HEADER_NAME,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-API-KEY"
                },
                In = ParameterLocation.Header
            },
            new string[]{}
        }
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<HandleResponseMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(SWAGGERFILE_PATH, $"{PROJECT_NAME} - {API_VERSION}");
    c.RoutePrefix = string.Empty;
});
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
