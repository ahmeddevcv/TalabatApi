using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services;

namespace Talabat.APIs.Controllers
{
    public class AccountsController : ApiBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountsController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService,IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }
        [HttpPost("login")]//Post : api/accounts/login
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            //find email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return BadRequest(new ApiResponse(401));
            //checkpassword
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return BadRequest(new ApiResponse(401));
            return new UserDto()
            {
                DisplayName = model.Email,
                Email = model.Email,
                //5 token
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
        }
        [HttpPost("Register")]//Post : api/accounts/Register
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {
            if(CheckEmailExists(model.Email).Result.Value) return BadRequest(new ApiValidationErrorResponse() { Errors=new string[] {"this email is already in user!!"} });
            var user = new AppUser()
            {
                DisplayName = model.DisplayName, //aya mohamed
                Email = model.Email, //aya.mohamed@linkdev.com
                UserName = model.Email.Split("@")[0], //aya.mohamed
                PhoneNumber = model.PhoneNumber, //01145
            };
            var result = await _userManager.CreateAsync(user, model.Password);//Pa$$w0rd
            //
            if (!result.Succeeded) return BadRequest(new ApiResponse(401));
            return new UserDto()
            {
                DisplayName = model.Email,
                Email = model.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
        }
        // 8 GetUserAddres
        //      {
        //"email": "ahmed.shaban@linkdev.com",
        //"password": "Pa$$w0rd"
        //}
        // {
        //   "email": "ahmed.shaban@linkdev.com",
        //   "password": "PA$$W0RD"
        //   }
        [Authorize]
        [HttpGet("get")] //Get : api/accounts/cu
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            //check email in claim
            //find it then return
            var email=User.FindFirstValue(ClaimTypes.Email);
            var user=await _userManager.FindByEmailAsync(email);
            return new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
        }
        //8 GetUserAddres
        [Authorize]
        [HttpGet("address")] //Get : api/accounts/address
        public async Task<ActionResult<AddressDto>> GetUserAddres()
         {
            //find current email //current user
            //var email=User.FindFirstValue(ClaimTypes.Email);
            //var user = await _userManager.FindByEmailAsync(email);
            var user = await _userManager.FindUserWithAddressByEmailAsync(User);
            //8 GetUserAddres mapping
            //return new AddressDto() //manual mapping
            //{

            //}
            var address = _mapper.Map<Address,AddressDto>(user.Address);
            return Ok(address);
        }
        [Authorize]
        [HttpPut("address")] //Put : api/accounts/address
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto updatedAddress)
        {
            #region repairWith_Aliaa
            var user = await _userManager.FindUserWithAddressByEmailAsync(User);
            if (user == null) return NotFound("User not found");
            var address = _mapper.Map<AddressDto, Address>(updatedAddress);
            if (user.Address == null)
            {
                user.Address = address;
            }
            else
            {
                address.Id = user.Address.Id;
                user.Address = address;
            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400));
            return Ok(updatedAddress);



            #endregion


            ///var address = _mapper.Map<AddressDto, Address>(updatedAddress);// (await) امتي ماحطهاش
            ///
            ///var user = await _userManager.FindUserWithAddressByEmailAsync(User);
            ///if (user.Address == null) return NotFound("User address not found");
            ///address.Id= user.Address.Id;
            ///user.Address= address;
            ///
            ///var result=await _userManager.UpdateAsync(user);
            ///if (!result.Succeeded)return BadRequest(new ApiResponse(400));
            ///return Ok(updatedAddress);
        }

        [HttpGet("emailexists")] //Get : api/accounts/emailexists?ahmed.shaban@linkdev.com
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }
    }
}
