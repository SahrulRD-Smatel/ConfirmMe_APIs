using ConfirmMe.Data;
using ConfirmMe.Dto;
using ConfirmMe.Extensions;
using ConfirmMe.Models;
using ConfirmMe.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using QuestPDF.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }); ;

// Register custom services
builder.Services.AddScoped<IApprovalRequestService, ApprovalRequestService>();
builder.Services.AddScoped<IApprovalFlowService, ApprovalFlowService>();
builder.Services.AddScoped<IBarcodeService, BarcodeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<ILetterService, LetterService>();


builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


// Configure EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    var secretKey = builder.Configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new ArgumentNullException("JwtSettings:SecretKey", "JWT secret key must be provided.");
    }

    // Decode dari Base64 string
    var keyBytes = Convert.FromBase64String(secretKey);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey))
    };

    // Mengambil token dari cookie jika tidak ada Authorization header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("token"))
            {
                context.Token = context.Request.Cookies["token"];
            }
            return Task.CompletedTask;
        }
    };

});

// Add Authorization Policies for Role-based Access Control
builder.Services.AddAuthorization(options =>
{
    // Policy for HRD or higher roles
    options.AddPolicy("IsHRDOrAbove", policy =>
        policy.RequireRole("HRD", "Manager", "Direktur"));

    // Policy for Manager or higher roles
    options.AddPolicy("IsManagerOrAbove", policy =>
        policy.RequireRole("Manager", "Direktur"));

    // Policy for Director only
    options.AddPolicy("IsDirector", policy =>
        policy.RequireRole("Direktur"));
});

// Swagger (OpenAPI) with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ConfirmMe API",
        Version = "v1",
        Description = "API for ConfirmMe app"
    });

    // Mengonfigurasi Swagger untuk memperlakukan Approvers sebagai array
    c.MapType<List<ApproverDto>>(() => new OpenApiSchema
    {
        Type = "array", // Menandakan bahwa ini adalah array
        Items = new OpenApiSchema
        {
            Type = "object", // Menandakan bahwa item di dalam array adalah objek
            Properties =
            {
                { "approverId", new OpenApiSchema { Type = "string" } },
                { "positionId", new OpenApiSchema { Type = "integer" } },
                { "approverName", new OpenApiSchema { Type = "string" } },
                { "approverEmail", new OpenApiSchema { Type = "string" } }
            }
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Masukkan token sebagai 'Bearer {your token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddHttpContextAccessor();

//CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://confirmme.my.id",
                                            "http://103.176.78.120"
                                )
                                .AllowCredentials() 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                                
                      });
});


QuestPDF.Settings.License = LicenseType.Community;


var app = builder.Build();

// Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfirmMe API V1");
        c.RoutePrefix = string.Empty;
    });
}


//Noted takut lupa DBSeeder ini jalan juga di production kalo mau bungkus di envisdevelopment kalo mau jalan di development aja
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<AppDbContext>();

//    await DbSeeder.Seed(context, services); // <== panggil seeder di sini
//}

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();

app.UseAuthentication(); // WAJIB sebelum UseAuthorization
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
