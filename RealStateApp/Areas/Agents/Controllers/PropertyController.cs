using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealStateApp.Core.Application.Dtos.Improvement;
using RealStateApp.Core.Application.Dtos.Properties;
using RealStateApp.Core.Application.Dtos.SaleType;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Application.ViewModels.Properties;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Interfaces;
using RealStateApp.Helpers;
using RealStateApp.Infraestructure.Identity.Entities;

namespace RealStateApp.Areas.Agents.Controllers
{
    [Area("Agents")]
    [Authorize(Roles = "AGENT")]
    public class PropertyController : Controller
    {
        private readonly IAgentService _agentService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImpromentService _improvementService;
        private readonly IPropertyPhotoRepository _propertyPhotoRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public PropertyController(
            IAgentService agentService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImpromentService improvementService,
            IPropertyPhotoRepository propertyPhotoRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _agentService = agentService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _propertyPhotoRepository = propertyPhotoRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var properties = await _agentService.GetAgentProperties(user.Id);
            var viewModels = _mapper.Map<List<AgentPropertyViewModel>>(properties.Where(p => p.Status == Core.Domain.Common.Enums.PropertyStatus.Available).ToList());

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View(new SavePropertyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePropertyViewModel vm)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"ModelState Error: {error}");
                    }
                    await LoadSelectLists();
                    return View(vm);
                }

                if (vm.Photos == null || !vm.Photos.Any())
                {
                    ModelState.AddModelError("Photos", "Debe seleccionar al menos una imagen");
                    await LoadSelectLists();
                    return View(vm);
                }

                if (vm.Photos.Count > 4)
                {
                    ModelState.AddModelError("Photos", "No puede seleccionar más de 4 imágenes");
                    await LoadSelectLists();
                    return View(vm);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Index", "Login", new { area = "" });
                }

                var dto = _mapper.Map<SavePropertyDto>(vm);
                var createdDto = await _agentService.CreateProperty(dto, user.Id);

                foreach (var photo in vm.Photos)
                {
                    var photoUrl = FileHelper.Upload(photo, createdDto.Id!.Value, "Properties");
                    var propertyPhoto = new PropertyPhoto
                    {
                        PropertyId = createdDto.Id.Value,
                        ImageUrl = photoUrl
                    };
                    await _propertyPhotoRepository.AddAsync(propertyPhoto);
                }

                TempData["SuccessMessage"] = "Propiedad creada exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear la propiedad: {ex.Message}");
                await LoadSelectLists();
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            await LoadSelectLists();

            var property = await _propertyPhotoRepository.GetPropertyWithDetails(id);
            if (property == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || property.AgentId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            var improvements = await _propertyImprovementRepository.GetListImprovementBYPorpertyId(id);
            var photos = await _propertyPhotoRepository.GetListPhotByPropertyId(id);

            var vm = new SavePropertyViewModel
            {
                Id = property.Id,
                PropertyTypeId = property.PropertyTypeId,
                SaleTypeId = property.SaleTypeId,
                Price = property.Price,
                SizeInMeters = property.SizeInMeters,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Description = property.Description,
                ImprovementIds = improvements.Select(i => i.ImprovementId).ToList(),
                ExistingPhotos = photos.Select(p => p.ImageUrl).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavePropertyViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(vm);
            }

            if (!vm.Id.HasValue)
            {
                ModelState.AddModelError("", "El Id de la propiedad es requerido");
                await LoadSelectLists();
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var dto = _mapper.Map<SavePropertyDto>(vm);
            await _agentService.UpdateProperty(dto);

            // Actualizar fotos si se subieron nuevas
            if (vm.Photos != null && vm.Photos.Any())
            {
                if (vm.Photos.Count > 4)
                {
                    ModelState.AddModelError("Photos", "No puede seleccionar más de 4 imágenes");
                    await LoadSelectLists();
                    return View(vm);
                }

                // Eliminar fotos antiguas
                var oldPhotos = await _propertyPhotoRepository.GetListPhotByPropertyId(vm.Id!.Value);
                foreach (var photo in oldPhotos)
                {
                    await _propertyPhotoRepository.DeleteAsync(photo.Id);
                }

                // Guardar nuevas fotos
                foreach (var photo in vm.Photos)
                {
                    var photoUrl = FileHelper.Upload(photo, vm.Id.Value, "Properties");
                    var propertyPhoto = new PropertyPhoto
                    {
                        PropertyId = vm.Id.Value,
                        ImageUrl = photoUrl
                    };
                    await _propertyPhotoRepository.AddAsync(propertyPhoto);
                }
            }

            TempData["SuccessMessage"] = "Propiedad actualizada exitosamente";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var property = await _propertyPhotoRepository.GetPropertyWithDetails(id);
            if (property == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || property.AgentId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            var dto = await _agentService.GetPropertyById(id);
            var vm = _mapper.Map<AgentPropertyViewModel>(dto);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _propertyPhotoRepository.GetPropertyWithDetails(id);
            if (property == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || property.AgentId != user.Id)
            {
                return RedirectToAction("AccessDenied", "Login", new { area = "" });
            }

            await _agentService.DeleteProperty(id);

            TempData["SuccessMessage"] = "Propiedad eliminada exitosamente";
            return RedirectToAction("Index");
        }

        private async Task LoadSelectLists()
        {
            var propertyTypes = await _propertyTypeService.GetAllList();
            var saleTypes = await _saleTypeService.GetAllList();
            var improvements = await _improvementService.GetAllList();

            ViewBag.PropertyTypes = propertyTypes?.Select(x => new SelectListItem 
            { 
                Value = x.Id.ToString(), 
                Text = x.Name 
            }).ToList() ?? new List<SelectListItem>();

            ViewBag.SaleTypes = saleTypes?.Select(x => new SelectListItem 
            { 
                Value = x.Id.ToString(), 
                Text = x.Name 
            }).ToList() ?? new List<SelectListItem>();

            ViewBag.Improvements = improvements?.Select(x => new SelectListItem 
            { 
                Value = x.Id.ToString(), 
                Text = x.Name 
            }).ToList() ?? new List<SelectListItem>();
        }
    }
}
