using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace TelerikMvcApp4.Controllers
{
    public class CarViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        [UIHint("ClientCategory")]
        public string CategoryName { get; set; }
    }


    public class HomeController : Controller
    {
        CarsDbContext db;
        public HomeController()
        {
            db = new CarsDbContext();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Cars_Read([DataSourceRequest] DataSourceRequest request)
        {
            return Json((from c in db.Cars
                         select new CarViewModel
                         {
                             Id = c.Id,
                             Name = c.Name,
                             CategoryId = c.CategoryId,
                             CategoryName = c.Category.Name,
                             //Category = new CategoryViewModel { Id = c.CategoryId, Name = c.Category.N }
                         }).ToDataSourceResult(request), JsonRequestBehavior.AllowGet);

        }


        public async Task<ActionResult> Categories()
        {
            var l = from c in await db.Categories.ToListAsync()
                    select new { Id = c.Id, Name = c.Name };

            return Json(l, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> Cars_Create([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CarViewModel> cars)
        {
            foreach (var car in cars)
            {
                if (car != null && ModelState.IsValid)
                {
                    var newCar = new Car { Name = car.Name };
                    if (car.CategoryId.HasValue)
                    {
                        newCar.CategoryId = car.CategoryId.Value;
                    }
                    else
                    {
                        var category = new Category { Name = car.CategoryName };
                        db.Entry(category).State = EntityState.Added;
                        newCar.Category = category;
                    }

                    db.Cars.Add(newCar);
                    await db.SaveChangesAsync();
                    car.Id = newCar.Id;

                }
            }
            await db.SaveChangesAsync();
            return Json(cars.ToDataSourceResult(request, ModelState));
        }


        [HttpPost]
        public async Task<ActionResult> Cars_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CarViewModel> cars)
        {
            foreach (var car in cars)
            {
                if (car != null && ModelState.IsValid)
                {
                    var dbCar = new Car { Id = car.Id, Name = car.Name };
                    if (car.CategoryId.HasValue)
                    {
                        dbCar.CategoryId = car.CategoryId.Value;
                    }
                    else
                    {
                        var category = new Category { Name = car.CategoryName };
                        db.Entry(category).State = EntityState.Added;
                        dbCar.Category = category;
                    }

                    db.Cars.Attach(dbCar);
                    db.Entry(dbCar).State = EntityState.Modified;

                }
            }
            await db.SaveChangesAsync();
            return Json(cars.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public async Task<ActionResult> Cars_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<CarViewModel> cars)
        {
            foreach (var carVm in cars)
            {
                var dbCar = new Car { Id = carVm.Id };
                db.Cars.Attach(dbCar);
                db.Cars.Remove(dbCar);
            }
            await db.SaveChangesAsync();
            return Json(cars.ToDataSourceResult(request));
        }

    }
}
