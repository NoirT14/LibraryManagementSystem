
using APIServer.Configs;
using APIServer.Data;
using APIServer.Middleware;
using APIServer.Repositories;
using APIServer.Repositories.Interfaces;
using APIServer.Service;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using APIServer.Service.Jobs;
using APIServer.DTO.Author;
using APIServer.DTO.Category;
using APIServer.DTO.Edition;
using APIServer.DTO.CoverType;
using APIServer.DTO.PaperQuality;
using APIServer.Config;
using Microsoft.OData.ModelBuilder;
using APIServer.Models;
using APIServer.DTO.Book;

namespace LibraryManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            builder.Services.AddControllers().AddOData(opt =>
                opt.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
                    .AddRouteComponents("odata", ODataConfig.GetEdmModel()));

            builder.Services.AddDbContext<LibraryDatabaseContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddControllers()
                .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                opt.JsonSerializerOptions.WriteIndented = true;
            });

            builder.Services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                );
            });

            //add jwt
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            });

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>(); // thêm lại
            builder.Services.AddScoped<IUserService, UserService>();   // thêm lại
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<IBookCopyService, BookCopyService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddHostedService<SessionCleanupService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped(typeof(APIServer.Repositories.Interfaces.IRepository<>), typeof(APIServer.Repositories.Repository<>));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICoverTypeService, CoverTypeService>();
            builder.Services.AddScoped<IEditionService, EditionService>();
            builder.Services.AddScoped<IPaperQualityService, PaperQualityService>();
            builder.Services.AddScoped<IPublisherService, PublisherService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthorService, AuthorService>();
            builder.Services.AddScoped<IBookVariantService, BookVariantService>();

            builder.Services.AddHostedService<NotificationJob>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");

            // Thêm middleware CORS ngay trước UseAuthorization
            app.UseCors("AllowLocalhost3000");

            app.UseAuthentication();
            app.UseMiddleware<BrowserFingerprintMiddleware>();
            app.UseAuthorization();

            app.MapControllers();

            app.UseCors("AllowAll");

            app.Run();

        }
    }
}
