using ImageUpload.Data;
using ImageUpload.Models;
using ImageUpload.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUpload.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Get All List
        public IActionResult Index()
        {
            var items = _context.Student.ToList();
            return View(items);
        }

        // Get Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(StudentViewModel vm)
        {
            string stringFileName = UploadFile(vm);
            var student = new Student
            {
                Name = vm.Name,
                Address = vm.Address,
                PhotoPath = stringFileName
            };
            _context.Student.Add(student);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Get : Student/Edit/1
        public IActionResult Edit(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var student = _context.Student.Find(Id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // Post : Student/Edit/1
        [HttpPost]
        public IActionResult Edit(int id, StudentViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var student = _context.Student.Find(vm.ID);
                student.Name = vm.Name;
                student.Address = vm.Address;

                if (vm.PhotoPath != null)
                {
                    if (student.PhotoPath != null)
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", student.PhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    student.PhotoPath = UploadFile(vm);
                }
                _context.Update(student);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // Get : Student/Delete/1
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imageModel = _context.Student.FirstOrDefault(m => m.ID == id);
            if(imageModel == null)
            {
                return NotFound();
            }
            return View(imageModel);
        }

        // Post : Student/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var imageModel = _context.Student.Find(id);

            // delete image from wwwroot/Images
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", imageModel.PhotoPath);
            if(System.IO.File.Exists(imagePath))
               System.IO.File.Delete(imagePath);
            // delete image from database
            _context.Student.Remove(imageModel);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Image Upload
        private string UploadFile(StudentViewModel vm)
        {
            string fileName = null;
            if (vm.PhotoPath.ContentType != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "Images\\1400\\NRC");

                //Check File Extension
                //string fileExtension="."+vm.PhotoPath.ContentType.Replace("image/","");

                // Create Directory
                if(!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                //fileName = vm.ID + "_" + vm.Name.Replace(" ", "_") + "_" + Guid.NewGuid().ToString() + "_" + vm.PhotoPath.FileName;
                //fileName = vm.Name.Replace(" ", "_") + "_" + Guid.NewGuid().ToString() + "_" + vm.PhotoPath.FileName;

                // Concat File Extension
                //fileName = vm.Name + "_front"+fileExtension;

                fileName = vm.Name + "_front.jpg";
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    vm.PhotoPath.CopyTo(fileStream);
                }

            }
            return fileName;
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
