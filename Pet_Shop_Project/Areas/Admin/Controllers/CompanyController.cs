using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;
using Pet_Shop_Project.Repository.IRepository;
using Utility;

namespace Pet_Shop_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<Company> companies = _unitOfWork.Company.GetAll();
            return View(companies);
        }

        // Get:  
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }
        }

        // Post: 
        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                    TempData["success"] = "Product updated successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        //[HttpPost]
        //public IActionResult Delete(int? id) 
        //{
        //    var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
        //    if (obj == null)
        //    {
        //        return Json(new { success = false, message = "Reeoe while deleting" });
        //    }

        //    _unitOfWork.Company.Remove(obj);
        //    _unitOfWork.Save();
        //    return Json(new { success = true, message = "Delete successful" });
        //}

        //[HttpDelete]
        //public IActionResult Delete(int id)
        //{
        //    Company company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Company.Remove(company);
        //    _unitOfWork.Save();
        //    return Json(new { success = true });
        //}

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var deleteCompany = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
            if (deleteCompany == null)
            {
                return NotFound();
            }
            return View(deleteCompany);
        }

        [HttpPost]
        public IActionResult Delete(int? id, string str = null)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
            //return Json(new { success = true, message = "Company deleted successfully." });
        }
    }
}