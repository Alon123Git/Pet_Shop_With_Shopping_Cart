using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using Models;
using Models.ViewModels;
using Pet_Shop_Project.Repository.IRepository;
using System;
using System.Diagnostics;
using System.Text;
using Utility;

namespace Pet_Shop_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class AnimalController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AnimalController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult HomePage()
        {
            var animals = _unitOfWork.Animal.GetAll().Take(2).ToList();
            return View(animals);
        }

        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<AnimalVM> objAnimalList = _unitOfWork.Animal.GetAll()
                .Select(a => new AnimalVM
                {
                    animal = a,
                    CategoryList = _unitOfWork.Animal.GetAll().Select(c => new SelectListItem
                    {
                        Text = c.AnimalName,
                        Value = c.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType.GetAll().Select(ct => new SelectListItem
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    })
                });
            return View(objAnimalList);
        }

        //Details function get: 
        [HttpGet]
        public IActionResult Details(int id)
        {
            Animal category = _unitOfWork.Animal.GetFirstOrDefault(x => x.Id == id);
            if (category == null)
            {
                ModelState.AddModelError("Name", "cannot match");
            }
            return View(category);
        }

        //Details function post:
        [HttpPost]
        public IActionResult Details(int id, string? str = null)
        {
            Animal category = _unitOfWork.Animal.GetFirstOrDefault(u => u.Id == id);
            return View(category);
        }

        //Add new animal function get: 
        [HttpGet]
        public IActionResult Create(int? id)
        {
            AnimalVM animalVM = new AnimalVM()
            {
                animal = new Animal(),
                CategoryList = _unitOfWork.Animal.GetAll().Select(i => new SelectListItem
                {
                    Text = i.AnimalName,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            if (id == null || id == 0)
            {
                // Create product
                return View(animalVM);
            }
            else
            {
                animalVM.animal = _unitOfWork.Animal.GetFirstOrDefault(u => u.Id == id);
                return View(animalVM);
                // Update product
            }
        }

        //Add new animal function get: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AnimalVM obj, IFormFile? animalImageUrl)
        {
            //if (ModelState.IsValid)
            //{
            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (animalImageUrl != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"Images");
                Console.WriteLine("Uploads path: " + uploads);
                var extention = Path.GetExtension(animalImageUrl.FileName);

                if (obj.animal.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, obj.animal.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extention), FileMode.Create))
                {
                    animalImageUrl.CopyTo(fileStreams);
                }

                obj.animal.ImageUrl = Url.Content(@"~/Images/" + fileName + extention);

            }
            if (obj.animal.Id == 0)
            {
                _unitOfWork.Animal.Add(obj.animal);
            }
            else
            {
                _unitOfWork.Animal.Update(obj.animal);
            }
            _unitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction(nameof(Admin));
            //}
            //return View(obj);
        }

        //Delete function get: 
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var deleteCategory = _unitOfWork.Animal.GetFirstOrDefault(u => u.Id == id);
            if (deleteCategory == null)
            {
                return NotFound();
            }
            return View(deleteCategory);
        }

        //Delete function post: 
        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int? id, string? str = null)
        {
            var obj = _unitOfWork.Animal.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleteing" });
            }

            _unitOfWork.Animal.Remove(obj);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Admin));
            //return Json(new { success = true, message = "Delete successful" });
        }

        public IActionResult Admin()
        {
            return View(_unitOfWork.Animal.GetAll());
        }
    }
}