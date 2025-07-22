
using APIServer.Service.Interfaces;
using APIServer.Service;
using APIServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using APIServer.Models;

namespace LibraryManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddControllers()
                .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        opt.JsonSerializerOptions.WriteIndented = true;
    });


            builder.Services.AddControllers().AddOData(opt =>
            {
                var builderOdata = new ODataConventionModelBuilder();
                builderOdata.EntitySet<Book>("Books");
                builderOdata.EntitySet<Category>("Categories");
                builderOdata.EntitySet<Author>("Authors");
                builderOdata.EntitySet<BookVolume>("BookVolumes");
                opt.AddRouteComponents("odata", builderOdata.GetEdmModel());
                opt.Select().Expand().Filter().OrderBy().Count().SetMaxTop(100);
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ILoanService, LoanService>();
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICoverTypeService, CoverTypeService>();
            builder.Services.AddScoped<IPaperQualityService, PaperQualityService>();
            builder.Services.AddScoped<IPublisherService, PublisherService>();


            builder.Services.AddDbContext<LibraryDatabaseContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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

            app.UseCors("AllowAll");

            app.Run();
        }
    }
}
