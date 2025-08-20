using CsvHelper;
using InventorySystemProject.Data;
using InventorySystemProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace InventorySystemProject.Controllers
{
    //[Authorize(Roles = "Admin")] // only access something from that controller if im logged in
    

    public class ItemReportDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public decimal Price { get; set; }
    }

    public class ItemsController : Controller
    {

        // list an item
        // variable to store your database connection
        private readonly InventorySystemProjectContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ItemsController(InventorySystemProjectContext context, UserManager<ApplicationUser> userManager)
        {
            // _context is your Database Connection Helper
            // Give me access to the Items table in the database — but let me use it as if it’s just a list of Item objects in C#.
            _context = context;
            _userManager = userManager; //how we access the current logged-in user.
        }

        // list items
        // View a list of all current items        
        public async Task<IActionResult> Index(int filter, string search, int currentPage)
        {    
            // create dropdowns
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");

            // start with the full query (as IQueryable so it can be filtered)
            var items = _context.Items
                                .Include(i => i.Categories)
                                .Include(i => i.Supplier)
                                .AsQueryable();

            
            // apply filter if provided for the dropdown
            if (filter != 0 )
            {
                items = items.Where(i =>
                    i.Supplier.Id == filter
                    );
            }

            // apply filter for the search box
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i =>
                      i.Name.Contains(search)
                      );
            }
  

            // skip items (10 per page)
            currentPage = Math.Max(currentPage, 1); // prevents from going below 1 , e.g. (-1, 1) = 1
            var skipItems = items.Skip(10*(currentPage - 1)); // (1 - 1) * 10 = 0 (show 1–10), (2 - 1) * 10 = 10 (show 11–20)
            var takeItems = skipItems.Take(10); // take ten items from the skipped list

            // 4. Execute query and return results
            var result = await takeItems.ToListAsync();
            return View(result);
        }


        //create and download csv file
        [HttpGet]
        public async Task<IActionResult> Report(int filter, string search, string download)
        {
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");

            var items =  _context.Items
                                .Include(i => i.Categories)
                                .Include(i => i.Supplier)
                                .AsQueryable();

            // 3. Apply filter if provided for the dropdown
            if (filter != 0)
            {
                items = items.Where(i =>
                    i.Supplier.Id == filter
                    );
            }

            // apply filter for the search box
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i =>
                      i.Name.Contains(search)
                      );
            }
          
            // logic to download the csv file
            if (download=="true")
            {
                // create DTO
                var reportData = await items
                    .Select(i => new ItemReportDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        CategoryName = i.Categories.Name,
                        SupplierName = i.Supplier.Name
                    })
                    .ToListAsync();

                // write the DTO into the csv file
                using var memoryStream = new MemoryStream(); // handle data in memory instead of saving it
                using var writer = new StreamWriter(memoryStream); // where to  write the csv file in this case in memory
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) // create a csv file
                {
                    csv.WriteRecords(reportData); // write the records from the DTO
                    writer.Flush(); // clears data from the memory

                }

                return File(memoryStream.ToArray(), "text/csv", "Report.csv"); // return the csv file to the browser
            }

            // Repopulate dropdowns
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");
            ViewData["Title"] = "request an item";

            var result = await items.ToListAsync();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> RequestItem(int id, int Quantity)
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");

            var items = await _context.Items
                               .Include(i => i.Categories)
                               .Include(i => i.Supplier)
                               .Include(i => i.ItemRequests)
                               .FirstOrDefaultAsync(x => x.Id == id);

            // pass the quantity in stock that in stored in the model
            // ViewBag.QuantityInStock = items.QuantityInStock;

            var vm = new ItemRequest
            {
                Items = items,
                ItemId = items.Id,
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestItem([Bind("ItemId, Quantity")] ItemRequest request)
        {
           
            // Repopulate the Item (and related data) so the view can display it again
            request.Items = await _context.Items
                .Include(i => i.Categories)
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId);
            

            // check if requested quantity exceeds stock
            if (request.Quantity > request.Items.QuantityInStock)
            {
                ViewBag.QuantityErrorMessage = $"You have requested {request.Quantity} items but you cannot request more than {request.Items.QuantityInStock} items in stock!";
                return View(request);
            }

            // check if this user has already requested this item
            var currentUserId = _userManager.GetUserId(User);
            bool alreadyRequested = await _context.ItemRequests
                .AnyAsync(r => r.ItemId == request.ItemId && r.ApplicationUserId == currentUserId);

            if (alreadyRequested)
            {
                ViewBag.RequestErrorMessage = "You have already requested this item!";
                return View(request); // show error instead of saving
            }


            // validate the model 
            if (ModelState.IsValid)
            {
                request.ApplicationUserId = _userManager.GetUserId(User); // set the user id manually
                request.RequestDate = DateTime.Now;
                request.Status = "Pending";
                request.Items.QuantityInStock -= request.Quantity;


                _context.Add(request);
                await _context.SaveChangesAsync();
            }


            return View(request);
        }


        // admin page
        public async Task<IActionResult> ItemRequestAdmin()
        {

            var items = await _context.ItemRequests
                    .Include(i => i.Items)
                    .Include(r => r.ApplicationUsers)
                    .ToListAsync();

            return View(items);

        }
        // delete requested item
        [HttpPost, ActionName("DeleteRequestedItem")]
        public async Task<IActionResult> DeleteRequestedItem(int Id)
        {
            var item = await _context.ItemRequests.FindAsync(Id);
            if (item == null)
            {
                return NotFound(); 
            }

            _context.ItemRequests.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("ItemRequestAdmin");
        }


        // Create
        // Add new items to your inventory
        public IActionResult Create()
        {
            // create a drowdown list 
            // these are for the many-to-one relationships
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name"); // 'Id' values for the option and 'Name' for the display label
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, Price, CategoryId, SupplierId", "QuantityInStock")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item); // keep the form with the form filled if there's any errors
        }

        // Edit or update existing items
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name"); // 'Id' values for the option and 'Name' for the display label
            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");
            var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,CategoryId")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        // Delete items
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }




    }
}
