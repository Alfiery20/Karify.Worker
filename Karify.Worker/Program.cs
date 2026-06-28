using Karify.Application.Models.Interface.Service;
using Karify.Application.Models.Services;
using Karify.Repository.Database;
using Karify.Application.Models.Interface.Repository;
using Karify.Worker;
using Karify.Repository.Repository;
using Karify.Infrastructure.Services;
using Karify.Application.Models.Interface;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient("MockApi", client => 
{ 
    client.BaseAddress = new Uri("https://localhost:7066/api/");
});

builder.Services.AddTransient<IProyectoRepository, ProyectoRepository>();
builder.Services.AddTransient<IProyectoService, ProyectoService>();
builder.Services.AddTransient<IUnprgExternalService, UnprgExternalService>();

builder.Services.AddTransient<DataBase>();

var host = builder.Build();
host.Run();
