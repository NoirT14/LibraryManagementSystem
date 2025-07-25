using APIServer.DTO.Author;
using APIServer.DTO.Book;
using APIServer.DTO.Category;
using APIServer.DTO.CoverType;
using APIServer.DTO.Edition;
using APIServer.DTO.PaperQuality;
using APIServer.DTO.User;
using APIServer.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace APIServer.Configs
{
    public class ODataConfig
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<AdminUserResponseDTO>("UsersOData");
            builder.EntitySet<BookVolumeDTO>("BookVolumes");
            builder.EntitySet<Notification>("notifications");
            builder.EntitySet<Book>("Books");

            builder.EntitySet<AuthorRespone>("Authors");
            builder.EntitySet<CategoryResponse>("Categories");
            builder.EntitySet<EditionResponse>("Editions");
            builder.EntitySet<CoverTypeResponse>("CoverTypes");
            builder.EntitySet<PaperQualityResponse>("PaperQuality");
            builder.EntitySet<HomepageBookDTO>("Book");

            return builder.GetEdmModel();
        }
    }
}
