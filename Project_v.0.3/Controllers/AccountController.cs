using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_v._0._3.Data;
using Project_v._0._3.Model;
using Project_v._0._3.ModelViews;
using System.Text.RegularExpressions;
using System.Web;

namespace Project_v._0._3.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly UserManager<AccountUserModel> userManager;
        private readonly RoleManager<AccountRoleModel> roleManager;
        private readonly SignInManager<AccountUserModel> signInManager;

        //constructor
        public AccountController(
            AppDbContext dbContext,
            UserManager<AccountUserModel> userManager,
            RoleManager<AccountRoleModel> roleManager,
            SignInManager<AccountUserModel> signInManager
            )
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
        }

        [HttpPost]
        [Route("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if(model != null && ModelState.IsValid)
            {
                if (PasswordMatch(model.PasswordHash) && IsValidEmail(model.Email) && UserNameMatch(model.UserName))
                {
                    if (Existes(model.Email, model.UserName))
                    {
                        var user = new AccountUserModel
                        {
                            UserName = model.UserName,
                            Email = model.Email
                        };

                        var result = await userManager.CreateAsync(user, model.PasswordHash);

                        if (result.Succeeded)
                        {
                            var token = await userManager.GenerateEmailConfirmationTokenAsync(user); // Create token form User
                            var confirmLink = Url.Action("RegistreationConfirm", "Account", new
                            {
                                Id = user.Id,
                                Token = HttpUtility.UrlEncode(token),
                            }, Request.Scheme);
                            //                        string text = "Please Confirm Registration at our sute";
                            var link = "<a href=\"" + confirmLink + "\">Confirm</a>";
                            return Ok(confirmLink);
                        }
                        return BadRequest(result.Errors + " Not Succeeded");
                    }
                    return BadRequest("Email or userNaem is Existes");
                }
                return BadRequest("password too short or email is valid");
                }
            return BadRequest(model);
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if(model != null && ModelState.IsValid)
            {
                var userLogin = await userManager.FindByNameAsync(model.UserName);
                if (userLogin != null && userLogin.EmailConfirmed)
                {
                    //  check if user and password is true or fales and if user login more 3 time block user
                    var result = await signInManager.PasswordSignInAsync(userLogin, model.PasswordHash, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return Ok(result + " => Succeefuly");
                    }
                    return BadRequest(result);
                }
                return BadRequest("userLogin is null => " + userLogin + "///or User is not Confirm Email => ");
            }
            return BadRequest("model is null or Model state is not valid");
        }




        private bool Existes(string email, string userName)
        {
            return dbContext.Users.Any(x => x.Email == email || x.UserName == userName);
        }
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$";
            // Create a regular expression object
            Regex regex = new Regex(pattern);

            // Use the regular expression to match the email
            Match match = regex.Match(email);

            // Return true if the email matches the pattern, otherwise false
            return match.Success;
        }
        private bool PasswordMatch(string password)
        {
            if (password.Length < 6)
            {
                return false;
            }
            return true;
        }
        private bool UserNameMatch(string userName)
        {
            if (userName.Length < 3)
            {
                return false;
            }
            string pattern = @"^[a-zA-Z0-9_]+$";
            // Create a regular expression object
            Regex regex = new Regex(pattern);

            // Use the regular expression to match the email
            Match match = regex.Match(userName);

            // Return true if the email matches the pattern, otherwise false
            return match.Success;
        }


        ////////////////////////////////////////////////////////////////////////////////////////
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RegistreationConfirm(string id, string Token)
        {
            // Check id or Token is empty
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(Token))
            {
                return BadRequest("ID or Token is Empty");
            }
            //  Find Id 
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                //  check the token and decode link and make  in datebase
                var result = await userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(Token));
                if (result.Succeeded)
                {
                    return Ok("Eamil Confirm Seccefuly");
                }
                return BadRequest(result.Errors + "Not Succeeded");
            }
            return BadRequest("Error RegistreationConfirm");
        }



    }
}
