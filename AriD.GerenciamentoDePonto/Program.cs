using AriD.GerenciamentoDePonto.Helpers;
using AriD.Servicos.DBContext;
using AriD.Servicos.Repositorios;
using AriD.Servicos.Repositorios.Interfaces;
using AriD.Servicos.Servicos;
using AriD.Servicos.Servicos.Interfaces;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using AriD.BibliotecaDeClasses.Configuracoes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("Email"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(RequestAuthenticationFilter));
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(5);
    options.IOTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(5);
    options.Cookie.MaxAge = TimeSpan.FromHours(5);
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MySQLDBContext>();

builder.Services.AddHttpClient();

builder.Services.AddScoped(typeof(IUsuarioAtual), typeof(UsuarioAtual));
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));

builder.Services.AddScoped(typeof(IServico<>), typeof(Servico<>));
builder.Services.AddScoped(typeof(IServicoDeFolhaDePonto), typeof(ServicoDeFolhaDePonto));
builder.Services.AddScoped(typeof(IServicoDeRelatorios), typeof(ServicoDeRelatorios));
builder.Services.AddScoped(typeof(IServicoDeEscala), typeof(ServicoDeEscala));
builder.Services.AddScoped(typeof(IServicoRegistroDePonto), typeof(ServicoRegistroDePonto));
builder.Services.AddScoped(typeof(IServicoDeArquivoFonteDeDados), typeof(ServicoDeArquivoFonteDeDados));
builder.Services.AddScoped(typeof(IServicoDeDashboard), typeof(ServicoDeDashboard));
builder.Services.AddScoped(typeof(IServicoDeAplicativo), typeof(ServicoDeAplicativo));
builder.Services.AddScoped(typeof(IServicoDeAplicativoDeRastreio), typeof(ServicoDeAplicativoDeRastreio));
builder.Services.AddScoped(typeof(IWhatsappService), typeof(WhatsappService));
builder.Services.AddScoped(typeof(IEmailService), typeof(EmailService));
builder.Services.AddScoped(typeof(IServicoDeServidor), typeof(ServicoDeServidor));
builder.Services.AddScoped(typeof(IServicoDeExportacaoFolhaPagamento), typeof(ServicoDeExportacaoFolhaPagamento));
builder.Services.AddScoped(typeof(IServicoBonus), typeof(ServicoBonus));
builder.Services.AddScoped(typeof(IServicoMonitoramentoRotas), typeof(ServicoMonitoramentoRotas));
builder.Services.AddScoped(typeof(IServicoNotificacao), typeof(FirebaseServico));
builder.Services.AddHttpClient<IServicoDeRoteirizacao, ServicoDeRoteirizacao>();

var app = builder.Build();

app.UseCors("AppCorsPolicy");

app.UseMiddleware<ExceptionMiddleware>();

var supportedCultures = new[] { new CultureInfo("pt-BR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();