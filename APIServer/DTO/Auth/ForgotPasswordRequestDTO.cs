using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIServer.DTO.Auth
{
    public class ForgotPasswordRequestDTO
    {
        [Required(ErrorMessage = "Username or Email is required")]
        public string UsernameorEmail { get; set; } = string.Empty;
    }
}
