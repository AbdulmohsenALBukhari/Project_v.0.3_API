using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_v._0._3.Data;
using Project_v._0._3.Model;
using Project_v._0._3.ModelViews;
using System.Security.Claims;
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
                    if (!Existes(model.Email, model.UserName))
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
                            return StatusCode(StatusCodes.Status200OK);
                        }
                        return BadRequest(result.Errors + " Not Succeeded");
                    }
                    return BadRequest("Email or userNaem is Existes");
                }
                return BadRequest("password too short or email is valid or user name to long maxLength 20");
                }
            return BadRequest(model);
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            await CreateAdmin();
            await CreateRoles();
            if (model != null && ModelState.IsValid)
            {
                var userLogin = await userManager.FindByNameAsync(model.UserName);
                if (userLogin != null && userLogin.EmailConfirmed)
                {
                    //  check if user and password is true or fales and if user login more 3 time block user
                    var result = await signInManager.PasswordSignInAsync(userLogin, model.PasswordHash, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        if (await roleManager.RoleExistsAsync("User"))
                        {
                            if (!await userManager.IsInRoleAsync(userLogin, "User"))
                            {
                                await userManager.AddToRoleAsync(userLogin, "User");
                            }
                        }
                        var roleName = await GetRoleNameByUserId(userLogin.Id);
                        if (roleName != null)
                            AddCookies(userLogin.UserName, roleName, userLogin.Id, model.RememberMe);
                        return Ok();
                    }
                    return BadRequest(result);
                }
                return BadRequest("userLogin is null => " + userLogin + "///or User is not Confirm Email => ");
            }
            return BadRequest("model is null or Model state is not valid");
        }

        [HttpGet]
        [Route("Logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
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
       

        ////////////////////////////////////////////////////////////////////////////////////////

        // check user name Validators
        private bool UserNameMatch(string userName)
        {
            if (userName.Length < 3 || userName.Length >= 20)
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
        // check email and user name if existes
        private bool Existes(string email, string userName)
        {
            return dbContext.Users.Any(x => x.Email == email || x.UserName == userName);
        }
        // check Email Validator
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
        // check password Validators
        private bool PasswordMatch(string password)
        {
            if (password.Length < 8)
            {
                return false;
            }
            return true;
        }
        //Create Admin if not Existes
        private async Task CreateAdmin()
        {
            var admin = await userManager.FindByNameAsync("Admin");
            if (admin == null)
            {
                var User = new AccountUserModel
                {
                    Email = "manager@admin.com",
                    UserName = "Admin",
                    EmailConfirmed = true
                };
                var x = await userManager.CreateAsync(User, "@Password125856479");
                if (x.Succeeded)
                {
                    if (await roleManager.RoleExistsAsync("Admin"))
                    {
                        await userManager.AddToRoleAsync(User, "Admin");
                    }
                }
            }
        }
        // get Role Name by Id
         private async Task<string> GetRoleNameByUserId(string id)
        {
            var userRole = await dbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == id);
            if (userRole != null)
            {
                return await dbContext.Roles.Where(x => x.Id == userRole.RoleId).Select(x => x.Name).FirstOrDefaultAsync();
            }
            return null;
        }
        //Create Cookies
        private async void AddCookies(string username, string roleName, string userId, bool remember)
        {
            var clim = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimIdentifier = new ClaimsIdentity(clim, CookieAuthenticationDefaults.AuthenticationScheme);

            if (remember)
            {
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = remember,
                    ExpiresUtc = DateTime.UtcNow.AddDays(15),
                };
                await HttpContext.SignInAsync
                    (
                        CookieAuthenticationDefaults.AuthenticationScheme,
                         new ClaimsPrincipal(claimIdentifier),
                         authProperties
                    );
            }
            else
            {
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = remember,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                };
                await HttpContext.SignInAsync
                    (
                        CookieAuthenticationDefaults.AuthenticationScheme,
                         new ClaimsPrincipal(claimIdentifier),
                         authProperties
                    );

            }
        }
        //create Roles for admin and user
        private async Task CreateRoles()
        {
            if (roleManager.Roles.Count() < 1)
            {
                var role = new AccountRoleModel
                {
                    Name = "Admin",
                };
                await roleManager.CreateAsync(role);

                role = new AccountRoleModel
                {
                    Name = "User",
                };
                await roleManager.CreateAsync(role);
            }
        }
    }
}
