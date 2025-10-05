using System.Security.Claims;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using JobSearcher.Account;
using JobSearcher.Api.MiddleWare;
using JobSearcher.Cv;
using JobSearcher.Data;
using JobSearcher.Job;
using JobSearcher.JobOpening;
using JobSearcher.Jwt;
using JobSearcher.Report;
using JobSearcher.UserReport;
using JobSearcher.UserSearchLink;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Amazon.Runtime;
using OpenAI.Chat;
using JobSearcher.AiAnalyzer;
using JobSearch.Emails;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey("picture", "picture");
    options.CallbackPath = "/signin-google";
});
builder.Services.AddScoped<IAccount, AccountMySql>();


Console.WriteLine($"secret {builder.Configuration["AWS:SecretAccessKey"]}");

var awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(builder.Configuration["AWS:AccessKeyId"], builder.Configuration["AWS:SecretAccessKey"]),
    Region = Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
};


string openAiApiKey = builder.Configuration["OpenAI:ApiKey"];

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


builder.Services.AddSingleton<IJobAnalyzerService, HuggingFaceJobAnalyzerService>();
builder.Services.AddScoped<ICvParserService, CvParserService>();

builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

builder.Services.AddScoped<ICvStorageService, CvStorageService>();
builder.Services.AddScoped<IUserCvStorageService, UserCvStorage>();
builder.Services.AddScoped<IUserFetchedLinkRepository, UserFetchedLinkRepository>();
builder.Services.AddScoped<IJobOpeningSearcher, JobOpeningSearcher>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<GlassDoorJobSearcher>();
builder.Services.AddSingleton<IndeedJobSearcher>();
builder.Services.AddSingleton<PracujJobSearcher>();
builder.Services.AddScoped<IndeedJobSearcherAdapter>();
builder.Services.AddScoped<GlassDoorJobSearchAdapter>();
builder.Services.AddScoped<PracujPlSearchAdapter>();
builder.Services.AddScoped<IUserReportService, UserReportService>();
builder.Services.AddScoped<IGenerateReportService, GenerateReportService>();
builder.Services.AddHostedService<ReportSetupBackgroundService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
