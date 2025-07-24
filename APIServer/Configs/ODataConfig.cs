using APIServer.DTO.Book;
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
            builder.EntitySet<Category>("Categories");
            builder.EntitySet<Author>("Authors");
            return builder.GetEdmModel();
        }
    }
}
