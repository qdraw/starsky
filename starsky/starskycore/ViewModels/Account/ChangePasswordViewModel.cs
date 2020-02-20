using System.ComponentModel.DataAnnotations;

namespace starskycore.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
	    /// <summary>
	    /// Password before change
	    /// </summary>
	    [StringLength(100, MinimumLength = 8)]
	    [DataType(DataType.Password)]
	    public string Password { get; set; }
	    
	    /// <summary>
	    /// The new password
	    /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "ChangedPassword")]
        public string ChangedPassword { get; set; }

	    /// <summary>
	    /// The new password confirmed
	    /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("ChangedPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ChangedConfirmPassword { get; set; }
    }
}
