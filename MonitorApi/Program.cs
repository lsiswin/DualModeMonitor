using MonitorApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlite<ApplicationDbContext>("Data Source=DualModeMonitorDB.db;Foreign Keys=True;Cache=Shared");

builder.Services.AddTransient<ApplicationDbContext>();
builder.Services.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddTransient<IDataPointService, DataPointService>();
builder.Services.AddTransient<IHumitureDeviceService, HumitureDeviceService>();
builder.Services.AddTransient<IModbusConfigService, ModbusConfigService>();

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

app.Run();
