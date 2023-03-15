using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using Pet_Shop_Project.Repository;
using Pet_Shop_Project.Repository.IRepository;
using System.Diagnostics;
using System.Security.Claims;
using Utility;

namespace Pet_Shop_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment _hostEnironment;

        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IHostEnvironment hostEnironment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _hostEnironment = hostEnironment;
        }
        //includeProperties: "Category, CoverType"
        public IActionResult Index()
        {
            IEnumerable<Animal> animalList = _unitOfWork.Animal.GetAll();
            return View(animalList);
        }

        public IActionResult Details(int animalid)
        {
            ShoppingCart ShopCart = new ShoppingCart()
            {
                count = 1,
                AnimalId = animalid,
                animal = _unitOfWork.Animal.GetFirstOrDefault(u => u.Id == animalid),
            };
            ShopCart.animal.PriceBeforeDiscount = ShopCart.animal.Price * 2;
            return View(ShopCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _unitOfWork.shoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.AnimalId == shoppingCart.AnimalId
                );

            if (cartFromDb == null)
            {
                _unitOfWork.shoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                if (HttpContext != null && HttpContext.Session != null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.shoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
                }
            }
            else
            {
                _unitOfWork.shoppingCart.IncrementCount(cartFromDb, shoppingCart.count);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}