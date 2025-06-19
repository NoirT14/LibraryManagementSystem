using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIServer.DTO.Auth
{
    public class AuthResult
    {
        public bool ÍsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
