using MyBGList;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts => opts.ResolveConflictingActions(apiDesc => apiDesc.First()));

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/error", () => Results.Problem());
app.MapGet("/error/test", () => { throw new Exception("test"); });
app.MapGet("/BoardGames", () => new[] {
            new BoardGame(){
            Id = 1,
            Name = "Axis & Allies",
            Year = 1981
            },
             new BoardGame(){
            Id = 2,
            Name = "Citadels",
            Year = 2000
            },
              new BoardGame(){
            Id = 3,
            Name = "Terraforming Mars",
            Year = 2016
            },
        });
app.MapControllers();


app.Run();

