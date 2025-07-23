using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIServer.DTO.Auth
{
    public class ForgotPasswordRequestDTO
    {
        public string UsernameorEmail { get; set; } = null!;
    }
}
