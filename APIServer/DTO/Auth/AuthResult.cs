using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIServer.DTO.Auth
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        // ✅ CHANGED: Use object instead of LoginResponseDTO for flexibility
        public object? Data { get; set; }
    }
}