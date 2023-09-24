using Microsoft.AspNetCore.Cors;
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
    app.UseExceptionHandler("/error");
}

// 中间件添加的顺序就是调用的顺序
app.UseHttpsRedirection();
// 使用默认的policy
app.UseCors();
// 使用其他命名policy
app.UseCors("AnyOrigin");
app.UseAuthorization();

// minimal apis
app.MapGet("/error", () => Results.Problem()).RequireCors("AnyOrigin");
app.MapGet("/error/test", () => { throw new Exception("test"); }).RequireCors("AnyOrigin");
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

