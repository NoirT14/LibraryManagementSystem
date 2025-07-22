
using APIServer.Configs;
using APIServer.Data;
using APIServer.DTO.Book;
using APIServer.Models;
using APIServer.Repositories;
using APIServer.Repositories.Interfaces;
using APIServer.Service;
using APIServer.Service.Interfaces;
using APIServer.Service.Jobs;
using APIServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Text;

namespace LibraryManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
    .AddOData(opt => opt
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .Expand()
        .SetMaxTop(100)
        .AddRouteComponents("api", GetEdmModel()));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddDbContext<LibraryDatabaseContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

          

            //add jwt
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
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

            //add authorization role
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            });

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
          

            // Thêm cấu hình CORS ở đây
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000",
                    policy => policy.WithOrigins("http://localhost:3000")
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
            });
            builder.Services.AddScoped<IAuthService, AuthService>(); 

            builder.Services.AddScoped<IBookVolumeService, BookVolumeService>();
            builder.Services.AddScoped(typeof(APIServer.Repositories.Interfaces.IRepository<>), typeof(APIServer.Repositories.Repository<>));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IBookVariantService, BookVariantService>();
            builder.Services.AddScoped<IBookService, BookService>();


            builder.Services.AddHostedService<NotificationJob>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");

            // Thêm middleware CORS ngay trước UseAuthorization
            app.UseCors("AllowLocalhost3000");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
        static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<BookVolumeDTO>("BookVolumes");
            builder.EntitySet<HomepageBookDTO>("Books");
            builder.EntitySet<Notification>("notifications");

            return builder.GetEdmModel();
        }
    }

}