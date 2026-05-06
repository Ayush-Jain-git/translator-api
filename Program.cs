using TranslatorAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});
//builder.Services.AddOpenApi();
builder.Services.AddHttpClient<TranslationService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//app.UseSwagger();
//app.UseSwaggerUI();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5082";
app.Urls.Add($"http://0.0.0.0:{port}");
//app.Urls.Add("http://0.0.0.0:5082");
app.MapControllers();


app.UseCors();
app.MapGet("/", () => "Translator API is running");
app.Run();

