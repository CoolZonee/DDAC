using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Areas.Identity.Data;

namespace WebApplication3.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<WebApplication3User> _userManager;
        private readonly SignInManager<WebApplication3User> _signInManager;

        public IndexModel(
            UserManager<WebApplication3User> userManager,
            SignInManager<WebApplication3User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "UserID")]
            public string UserID { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "You must enter the name first before submitting your form!")]
            [StringLength(256, ErrorMessage = "You must enter the value between 6 - 256 chars", MinimumLength = 6)]
            [Display(Name = "Your Full Name")] //label
            public string customerfullname { get; set; } 

            [Required] [Display(Name = "Your DOB")] 
            [DataType(DataType.Date)] 
            public DateTime DoB { get; set; } 

            [Required(ErrorMessage = "You must enter the age first before submitting your form!")] 
            [Range(18, 100, ErrorMessage ="You must be 18 years old when register this member!")] 
            [Display(Name = "Your Age")] //label
            public int age { get; set; } 

            [Required]
            [DataType(DataType.MultilineText)] 
            [Display(Name = "Your Address")] 
            public string Address { get; set; }
        }

        private async Task LoadAsync(WebApplication3User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                UserID = user.Id,
                PhoneNumber = phoneNumber,
                customerfullname = user.CustomerFullName,
                age = user.CustomerAge,
                Address = user.CustomerAddress,
                DoB = user.CustomerDOB
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            if (Input.customerfullname != user.CustomerFullName) { user.CustomerFullName = Input.customerfullname; }
            if (Input.DoB != user.CustomerDOB) { user.CustomerDOB = Input.DoB; }
            if (Input.Address != user.CustomerAddress) { user.CustomerAddress = Input.Address; }
            if (Input.age != user.CustomerAge) { user.CustomerAge = Input.age; }
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
