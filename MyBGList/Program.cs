using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyBGList.Constants;
using MyBGList.GraphQL;
using MyBGList.gRPC;
using MyBGList.Models;
using MyBGList.Swagger;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    // Setup a HTTP/2 endpoint without TLS.
//    options.ListenLocalhost(5000, o => o.Protocols =
//        HttpProtocols.Http2);
//});

builder.Logging
        .ClearProviders()
        .AddSimpleConsole()
        //.AddSimpleConsole(options => {

        //    options.SingleLine = true;
        //    options.TimestampFormat = "HH:mm:ss ";
        //    options.UseUtcTimestamp = true;
        //})
        .AddDebug()
        .AddJsonConsole(options => {

            options.TimestampFormat = "HH:mm";
            options.UseUtcTimestamp = true;
        });

// Add services to the container.
builder.Services.AddCors(options => {

    options.AddDefaultPolicy(cfg => {

        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();

    });

    options.AddPolicy(name: "AnyOrigin", cfg =>
    {
        cfg.AllowAnyOrigin();
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();

    });

});

builder.Host.UseSerilog((ctx, lc) => {

    lc.ReadFrom.Configuration(ctx.Configuration);
    lc.Enrich.WithMachineName();
    lc.Enrich.WithThreadName();
    lc.Enrich.WithThreadId();
    lc.WriteTo.File("Logs/log.txt",
        outputTemplate:
            "{Timestamp:HH:mm:ss} [{Level:u3}] " +
            "[{MachineName} #{ThreadId} {ThreadName} ] " +
            "{Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day);

    lc.WriteTo.File("Logs/errors.txt",
     outputTemplate:
         "{Timestamp:HH:mm:ss} [{Level:u3}] " +
         "[{MachineName} #{ThreadId} {ThreadName}] " +
         "{Message:lj}{NewLine}{Exception}",
     restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
     rollingInterval: RollingInterval.Day);
    lc.WriteTo.MSSqlServer(
          //restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
          connectionString: ctx.Configuration.GetConnectionString("DefaultConnection"),
          sinkOptions: new MSSqlServerSinkOptions() {

              TableName = "LogEvents",
              AutoCreateSqlTable = true
          },
          columnOptions: new ColumnOptions() {
              AdditionalColumns = new SqlColumn[] {
                  new SqlColumn(){

                      ColumnName = "SourceContext",
                      PropertyName = "SourceContext",
                      DataType = System.Data.SqlDbType.NVarChar
                  }

              }

          }
    );

}, writeToProviders: true);

builder.Services.AddControllers(options =>
{
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        (x) => $"The value '{x}' is invalid.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        (x) => $"The field {x} must be a number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (x, y) => $"The value '{x}' is not valid for {y}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => $"A value is required.");

    options.CacheProfiles.Add("NoCache", new CacheProfile() { NoStore = true});
    options.CacheProfiles.Add("Any-60", new CacheProfile() {
        Location = ResponseCacheLocation.Any,
        Duration = 60
    });
    options.CacheProfiles.Add("Client-120", new CacheProfile() {
        Location = ResponseCacheLocation.Client,
        Duration = 120
    });

});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts => {
    opts.EnableAnnotations();
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(System.IO.Path.Combine(AppContext.BaseDirectory, xmlFilename));


    opts.ResolveConflictingActions(apiDesc => apiDesc.First());
    opts.ParameterFilter<SortOrderFilter>();
    opts.ParameterFilter<SortColumnFilter>();

    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opts.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    opts.OperationFilter<AuthRequirementFilter>();
    opts.DocumentFilter<CustomDocumentFilter>();
    opts.RequestBodyFilter<PasswordRequestFilter>();
    opts.RequestBodyFilter<UsernameRequestFilter>();
    opts.SchemaFilter<CustomKeyValueFilter>();
});

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );

});

builder.Services.AddIdentity<ApiUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;

}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options => {

    options.DefaultAuthenticateScheme = options.DefaultChallengeScheme = options.DefaultForbidScheme = options.DefaultScheme =
    options.DefaultSignInScheme = options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options => {

    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
            )
    };

});
// rbac 是检测是否为某个claim
// cbac 基于policy 是检测是否是多个cliam中的一个或者同时多个claim
// pbac 基于iRequirements 还能检测特定的值范围
// rbac 实现不需要加下面的 
// ......

// CBAC
builder.Services.AddAuthorization(options => {

    // cbac
    options.AddPolicy("ModeratorWithMobilePhone", policy => policy.RequireClaim(ClaimTypes.Role, RoleNames.Moderator).RequireClaim(ClaimTypes.MobilePhone));
    // pbac
    options.AddPolicy("MinAge18", policy =>
        policy
            .RequireAssertion(ctx =>
                ctx.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth)
                && DateTime.ParseExact(
                    "yyyyMMdd",
                    ctx.User.Claims.First(c =>
                        c.Type == ClaimTypes.DateOfBirth).Value,
                    System.Globalization.CultureInfo.InvariantCulture)
                    >= DateTime.Now.AddYears(-18)));
});


builder.Services.AddResponseCaching(options => {
    options.MaximumBodySize = 32 * 1024 * 1024;
    options.SizeLimit = 50 * 1024 * 1024;
    options.UseCaseSensitivePaths = true;

});

builder.Services.AddMemoryCache();

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName = "AppCache";
});

//builder.Services.AddStackExchangeRedisCache(options => {

//    options.Configuration = builder.Configuration["Redis:ConnectionString"];

//});

//builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

builder.Services.AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

builder.Services.AddGrpc();

var app = builder.Build();


// Configure the HTTP request pipeline.
//app.Environment.IsDevelopment()
if (app.Configuration.GetValue<bool>("UseSwagger"))
    {
    app.UseSwagger();
    app.UseSwaggerUI();
   
}
if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage")) {
    app.UseDeveloperExceptionPage();
}
else {
    // instead of delegating the exception handling process to a custom endpoint
    //app.UseExceptionHandler("/error");

    app.UseExceptionHandler(action => {

        action.Run(async (context) => {
            var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
            // TODO: logging sending notifications, and more
            var details = new ProblemDetails();
            details.Detail = exceptionHandler?.Error.Message;
            details.Extensions["traceId"] =
                    System.Diagnostics.Activity.Current?.Id
                      ?? context.TraceIdentifier;

            if (exceptionHandler?.Error is NotImplementedException)
            {
                details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.2";
                details.Status = StatusCodes.Status501NotImplemented;
            }
            else if (exceptionHandler?.Error is TimeoutException)
            {
                details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.5";
                details.Status = StatusCodes.Status504GatewayTimeout;
            }
            else
            {
                details.Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                details.Status = StatusCodes.Status500InternalServerError;
            }
            //return Results.Problem(details);
            await context.Response.WriteAsJsonAsync(details);
            Console.WriteLine("-------");
            //await context.Response.WriteAsync(
            //System.Text.Json.JsonSerializer.Serialize(details));

        });

    });
}

// 中间件添加的顺序就是调用的顺序
app.UseHttpsRedirection();
// 使用默认的policy
app.UseCors();
// 使用其他命名policy
app.UseCors("AnyOrigin");

app.UseResponseCaching();

// 中间件的顺序会影响http的request pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<GrpcService>();

app.Use((context, next) => {
    //context.Response.Headers["cache-control"] = "no-cache, no-store";
    context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue() {

        NoCache = true,
        NoStore = true
    };
    return next.Invoke();
});

// minimal apis
// 端点路由中间件是app 级别
//app.MapGet("/error", () => Results.Problem()).RequireCors("AnyOrigin");
app.MapPost("/error", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)] (HttpContext context) =>
{
    var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
    // TODO: logging sending notifications, and more
    var details = new ProblemDetails();
    details.Detail = exceptionHandler?.Error.Message;
    details.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id
              ?? context.TraceIdentifier;
    details.Type =
        "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    details.Status = StatusCodes.Status500InternalServerError;
    return Results.Problem(details);

});
app.MapGet("/error", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)] (HttpContext context/*Microsoft.Extensions.Logging.ILogger logger*/) => {
    var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
    // TODO: logging sending notifications, and more
    var details = new ProblemDetails();
    details.Detail = exceptionHandler?.Error.Message;
    details.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id
              ?? context.TraceIdentifier;
    details.Type =
        "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    details.Status = StatusCodes.Status500InternalServerError;
    //logger.LogError(CustomLogEvents.Error_Get, exceptionHandler?.Error,"An unhandled exception occurred.");
    app.Logger.LogError(CustomLogEvents.Error_Get, exceptionHandler?.Error, "An unhandled exception occurred.");
    return Results.Problem(details);

});
app.MapGet("/error/test/501",
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () =>
    { throw new NotImplementedException("test 501"); });

app.MapGet("/error/test/504",
    [EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () =>
    { throw new TimeoutException("test 504"); });
app.MapGet("/error/test1", () => { throw new Exception("test"); }).RequireCors("AnyOrigin");
app.MapPost("/error/test1", () => { throw new Exception("test post"); }).RequireCors("AnyOrigin");
app.MapGet("/minimal-error", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)] () => Results.Problem());
app.MapGet("/minimal-error/test", [EnableCors("AnyOrigin")][ResponseCache(NoStore =true)] () => Results.Problem());
//app.MapGet("/BoardGames", () => new[] {
//            new BoardGame(){
//            Id = 1,
//            Name = "Axis & Allies",
//            Year = 1981
//            },
//             new BoardGame(){
//            Id = 2,
//            Name = "Citadels",
//            Year = 2000
//            },
//              new BoardGame(){
//            Id = 3,
//            Name = "Terraforming Mars",
//            Year = 2016
//            },
//        });


app.MapGet("/cod/test", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)] ()=>
Results.Text("<script>" +
    "window.alert('Your Client supports javascript!" +
    "\\r\\n\\r\\n" +
    $"Server time (UTC): {DateTime.UtcNow.ToString("O")}" +
    "//r//n"+
     "Client time (UTC): ' + new Date().toISOString());" +
        "</script>" +
        "<noscript>Your client does not support JavaScript</noscript>",
        "text/html"));


app.MapGet("/cache/test/1",
    [EnableCors("AnyOrigin")]
    (HttpContext context) => {
        context.Response.Headers["cache-control"] = "no-cache, no-store";
        return Results.Ok();
    });

app.MapGet("/cache/test/2",
    [EnableCors("AnyOrigin")]
    (HttpContext context) => {
        //context.Response.Headers["cache-control"] = "no-cache, no-store";
        return Results.Ok();
    });

app.MapGet("/auth/test/1", [Authorize] [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)]
[SwaggerOperation(
 Tags = new[] { "Auth"},
    Summary = "Auth test #1 (authenticated users).",
    Description = "Returns 200 - OK if called by an authenticated user regardless of its role(s).")]
    [SwaggerResponse(StatusCodes.Status200OK, "Authorized")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not authorized")]
() => {

    return Results.Ok("You are authorized!");
});
app.MapGet("/auth/test/2", [Authorize(Roles = RoleNames.Moderator)][EnableCors("AnyOrigin")][ResponseCache(NoStore = true)]
() => { return Results.Ok("You are authorized!"); });
app.MapGet("/auth/test/3", [Authorize(Roles = RoleNames.Administrator)][EnableCors("AnyOrigin")][ResponseCache(NoStore = true)]
() => { return Results.Ok("You are authorized!"); });
app.MapGet("/auth/test/4",
    [Authorize(Roles = RoleNames.SuperAdmin)]
[EnableCors("AnyOrigin")]
[ResponseCache(NoStore = true)] () =>
    {
        return Results.Ok("You are authorized!");
    });




// controllers
app.MapControllers().RequireCors("AnyOrigin");


app.Run();

