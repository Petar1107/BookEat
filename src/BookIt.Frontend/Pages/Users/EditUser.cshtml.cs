﻿using AutoMapper;
using BookIt.Application.Models.User;
using BookIt.Application.Services;
using BookIt.Core.Entities.Identity;
using BookIt.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookIt.Frontend.Pages.Users
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class EditUserModel : PageModel
    {
        private IUserService _userService;
        private IUserRepository _userRepository;
        private IWebHostEnvironment _webEnv;
        private IMapper _mapper;
        private UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IRestaurantRepository _restaurantRepository;

        public EditUserModel(IUserService userService,
            IUserRepository userRepository,
            IWebHostEnvironment environment,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IRestaurantRepository restaurantRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
            _webEnv = environment;
            _mapper = mapper;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _restaurantRepository = restaurantRepository;
        }

        [FromRoute]
        public string Id { get; set; }

        [BindProperty]
        public UpdateUserModel EditUser { get; set; }

        [BindProperty]
        public IEnumerable<ListUsersModel> Managers => _userService.GetAllManagers();

        public void OnGet()
        {
            var user = _userRepository.GetUserByIdAsync(Id).Result;
            var role = _userManager.GetRolesAsync(user).Result.ToList().First();
            EditUser = _mapper.Map<UpdateUserModel>(user);
            EditUser.Role = role;
        }

        public async Task<IActionResult> OnPostEdit()
        {
            if (!ModelState.IsValid) { return Page(); }

            EditUser.Id = Id;
            var user = _userService.EditUser(EditUser);
            var role = _userManager.GetRolesAsync(user).Result.First().ToString();
            await _userManager.RemoveFromRoleAsync(user, role);
            await _userManager.AddToRoleAsync(user, EditUser.Role);
       
            return RedirectToPage("ViewAllUsers");
        }

        public IActionResult OnPostDelete()
        {
            var user = _userRepository.GetUserByIdAsync(Id).Result;
            var role = _userManager.GetRolesAsync(user).Result.ToList().First();
            var managerRestaurant = _restaurantRepository.GetRestaurantByManagerIdAsync(Id).Result;
            if (role == "Manager" && managerRestaurant != null)
            {
                ViewData["Message"] = string.Format($"{user.UserName} is manager of a restaurant {managerRestaurant.RestaurantName} and cannot be deleted!");
                return Page();
            }
            else
            {
                _userService.DeleteUser(Id);
            }
            return RedirectToPage("ViewAllUsers");

        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
