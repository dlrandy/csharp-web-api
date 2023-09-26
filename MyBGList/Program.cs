﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBGList.Models;
using MyBGList.Swagger;

var builder = WebApplication.CreateBuilder(args);

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
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts => {

    opts.ResolveConflictingActions(apiDesc => apiDesc.First());
    opts.ParameterFilter<SortOrderFilter>();
    opts.ParameterFilter<SortColumnFilter>();
});

builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );

});


//builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

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
app.UseAuthorization();

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
app.MapGet("/error", [EnableCors("AnyOrigin")][ResponseCache(NoStore = true)] (HttpContext context) => {
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
// controllers
app.MapControllers().RequireCors("AnyOrigin");


app.Run();

