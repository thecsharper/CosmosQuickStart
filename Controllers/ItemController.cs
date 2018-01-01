namespace quickstartcore.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using quickstartcore.Models;

    public class ItemController : Controller
    {
        private IDocumentDBRepository<Item> _repository;

        public ItemController(IDocumentDBRepository<Item> repository)
        {
            _repository = repository;
        }

        [ActionName("Index")]
        public async Task<IActionResult> Index()
        {            
            var items = await _repository.GetItemsAsync(d => !d.Completed);

            return View(items);
        }
        
        [ActionName("Create")]
        public IActionResult CreateAsync()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                await _repository.CreateItemAsync(item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("Id,Name,Description,Completed")] Item item)
        {
            if (ModelState.IsValid)
            {
                await _repository.UpdateItemAsync(item.Id, item);
                return RedirectToAction("Index");
            }

            return View(item);
        }

        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Item item = await _repository.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Item item = await _repository.GetItemAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind("Id")] string id)
        {
            await _repository.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            Item item = await _repository.GetItemAsync(id);
            return View(item);
        }

        [ActionName("StoredProcedure")]
        public ActionResult CreateStoredProcedure()
        {
            return View();
        }

        [HttpPost]
        [ActionName("StoredProcedure")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisplayStoredProcedure()
        {
            var result = await _repository.GetStoredProcedureResult();

            var model = new StoredProcedureResult
            {
                Result = result
            };

            return View(model);
        }

        [ActionName("Trigger")]
        public ActionResult CreateTriggerResult()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Trigger")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisplayTriggerResult()
        {
            var result = await _repository.GetTriggerResult();

            var model = new TriggerResult
            {
                Result = result
            };

            return View(model);
        }

    }
}