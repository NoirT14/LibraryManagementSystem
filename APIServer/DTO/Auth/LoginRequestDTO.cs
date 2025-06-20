using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIServer.DTO.Auth
{
    public class LoginRequestDTO
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
