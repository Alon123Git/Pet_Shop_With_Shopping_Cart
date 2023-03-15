using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Pet_Shop_Project.Repository.IRepository;
using Utility;

namespace Pet_Shop_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult HomePage()
        {
            var animals = _unitOfWork.Animal.GetAll().Take(2).ToList();
            return View(animals);
        }

        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<CoverType> objCategoryList = _unitOfWork.CoverType.GetAll();
            return View(objCategoryList);
        }
        //Details function get: 
        [HttpGet]
        public IActionResult Details(int id)
        {
            CoverType coverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
            if (coverType == null)
            {
                ModelState.AddModelError("Name", "cannot match");
            }
            return View(coverType);
        }

        //Details function post:
        [HttpPost]
        public IActionResult Details(int id, string? str = null)
        {
            CoverType coverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            return View(coverType);
        }

        //Add new animal function get: 
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //Add new animal function get: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        //Edit function get:  
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);
            if (coverType == null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        //Edit function post: 
        [HttpPut]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj, string selectedcomm)
        {
            if (ModelState.IsValid == true)
            {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";

                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        //Delete function get: 
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var deleteCoverType = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (deleteCoverType == null)
            {
                return NotFound();
            }
            return View(deleteCoverType);
        }

        //Delete function post: 
        [HttpDelete, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id, string? str = null)
        {
            var obj = _unitOfWork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.CoverType.Remove(obj);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Admin()
        {
            return View(_unitOfWork.CoverType.GetAll());
        }
    }
}