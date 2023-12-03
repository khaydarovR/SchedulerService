using MVVMapp.App.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var licen = @"IRONSUITE.RAZIL.KHAYKA.GMAIL.COM.8907-4DEC981D52-BQLOMXHMWR6E4L-IJYRKAMWS6OL-FBBD4LLBOXAL-7RK2YXYLG5XI-BZJDMOZWHEVC-G5HALW45B2LS-KDMALC-TVQ4WZMJQT2LEA-DEPLOYMENT.TRIAL-XZ3JK6.TRIAL.EXPIRES.18.DEC.2023";
IronXL.License.LicenseKey = licen;

System.Environment.SetEnvironmentVariable("WT", "down");
app.MapGet("/ping", () => "Pong");

app.Run();
