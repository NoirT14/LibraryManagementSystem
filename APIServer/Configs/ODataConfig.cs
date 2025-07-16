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

            return builder.GetEdmModel();
        }
    }
}
