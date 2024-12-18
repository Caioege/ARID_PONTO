using AriD.Servicos.DBContext;
using AriD.Servicos.Repositorios;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos;
using AriD.Servicos.Servicos.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MySQLDBContext>();

builder.Services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
builder.Services.AddScoped(typeof(IServico<>), typeof(Servico<>));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();