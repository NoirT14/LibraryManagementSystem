
using APIServer.Data;
using APIServer.Service.Interfaces;
using APIServer.Service;
using Microsoft.EntityFrameworkCore;
using APIServer.Service.Jobs;
using Microsoft.AspNetCore.OData;
using APIServer.DTO.Notification;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using APIServer.Models;
using APIServer.Repositories;
using APIServer.Services;
using APIServer.DTO.Book;

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


            // Thêm middleware CORS ngay trước UseAuthorization
            app.UseCors("AllowLocalhost3000");

            app.UseAuthorization();

            app.MapControllers();

            Console.WriteLine("ConnectionString in use: " + builder.Configuration.GetConnectionString("DefaultConnection"));

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