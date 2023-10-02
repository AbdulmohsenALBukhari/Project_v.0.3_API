using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project_v._0._3.Data;
using Project_v._0._3.Model;
using Project_v._0._3.ModelViews;

namespace Project_v._0._3.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    //[Authorize]
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
            if(ModelState.IsValid)
            {
                var user = new AccountUserModel
                {
                    UserName = model.UserName,
                    Email = model.Email
                };

                var result = await userManager.CreateAsync(user,model.PasswordHash);

                if (result.Succeeded)
                {
                    return Ok(result);
                }
                return BadRequest(result.Errors);
            }
            return BadRequest(ModelState.ErrorCount);
        }

    }
}
