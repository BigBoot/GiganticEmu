
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GiganticEmu.Web
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "Please Enter Username..")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = default!;

        [Required(ErrorMessage = "Please Enter Email...")]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Please Enter Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Pwd { get; set; } = default!;

        [Required(ErrorMessage = "Please Enter the Confirm Password...")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Pwd")]
        public string ConfirmPwd { get; set; } = default!;

        public string? DiscordToken { get; set; }

    }
}
