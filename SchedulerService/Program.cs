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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var licen = @"IRONSUITE.RAZIL.KHAYKA.MAIL.RU.19894-34FD0E582A-NBHMM-Q2EBLLVD65Q2-AQ63RSXQGAYB-22VWC3EXVUSY-XZH5245AVWNY-Z7P4VXGRYKPM-VNZO2WMFQA4Y-CSVZ3X-TZVA5SISCVGKUA-DEPLOYMENT.TRIAL-4RAMTQ.TRIAL.EXPIRES.18.OCT.2023";
IronXL.License.LicenseKey = licen;

System.Environment.SetEnvironmentVariable("WT", "down");

app.Run();
