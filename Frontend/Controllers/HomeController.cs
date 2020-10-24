using System.Diagnostics;
using System.Linq;
using Frontend.Utilities;
using DataComponent.Repositories.Interfaces;
using DomainModels.Models;
using DomainModels.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using MongoDB.Driver;
using DataComponent.Repositories;

namespace Frontend.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IUserRepository _userRepository;

		public HomeController(ILogger<HomeController> logger, IUserRepository userRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
		}


		public IActionResult Index()
		{
			return View("Users");
		}


		[HttpPost]
		public IActionResult LoadUserData()
		{
			var sortDefinition = new SortDefinitionBuilder<User>().Descending(u=>u.CreatedAt);

			var users = _userRepository.GetAllSync(sortDefinition);
			
			if (users.Count > 0)
			{
				//return View("Users", new UsersViewModel
				//{
				//	Users = users.ToList()
				//});
				return Json(new { data = users });
				
			}

			var usersFromJson = new GetUsersFromJson("users.json").Execute();
			_userRepository.AddManySync(usersFromJson);
			users = usersFromJson.ToList();

			//return View("Users", new UsersViewModel { Users = users.ToList() });
			return Json(new { data = users });
		}

		[Route("CreateUser")]
		[HttpPost]
		public IActionResult CreateUser(User user)
		{
			if(ModelState.IsValid)
			{
				ViewBag.SuccessMsg = "successfully Created User";
				_userRepository.AddSync(user);
				ModelState.Clear();

			}


			return View();
		}

		[Route("CreateUser")]
		public IActionResult CreateUser()
		{
			ViewBag.SuccessMsg = "";
			return View();
		}


		[HttpGet]
		public ActionResult EditUser(string id)
		{
	
			var user = _userRepository.GetSingleByExpressionSync(u => u.Id == id);
			return View("EditUser", user);
		}

		[HttpPost]
		public ActionResult EditUser(User user)
		{
			if (ModelState.IsValid)
			{
				_userRepository.ReplaceOneSync(user.Id, user);
				return RedirectToAction("Index");
			}
			else
			{
				return View("EditUser", user);
			}
		}



		[HttpDelete]
		public JsonResult Delete(string id)
		{
			_userRepository.DeleteSync(x => x.Id == id);
			return new JsonResult(new
			{
				success = true,
				responseText = "User successfully deleted!"
			});
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}
	}
}