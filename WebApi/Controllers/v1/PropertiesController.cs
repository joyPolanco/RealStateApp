using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.Features.Property.Queries.GetAll;
using RealStateApp.Core.Application.Features.Property.Queries.GetAllWithInclude;
using RealStateApp.Core.Application.Features.Property.Queries.GetByCode;
using RealStateApp.Core.Application.Features.Property.Queries.GetById;
using Swashbuckle.AspNetCore.Annotations;

namespace RealStateWebApi.Controllers.v1
{
    /// <summary>
    /// API Propiedades V 1.0
    /// Controlador de consulta de Propiedades
    /// </summary>
    public class PropertiesController : BaseApiController
    {

        [HttpGet]
        [Authorize(Roles = "ADMIN,DEVELOPER")]
        [SwaggerOperation(
            Summary = "Obtiene todas las propiedades",
            Description = "Devuelve una lista de todas las propiedades registradas en el sistema."
        )]
        public async Task<IActionResult> GetAll()
        {
            var response = await Mediator.Send(new GetAllPropertiesQuery());

            if (response == null || response.Count == 0)
            {
                return NoContent();
            }

            return Ok(response);
        }
        [HttpGet("with-details")]
        [Authorize(Roles = "ADMIN,DEVELOPER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtiene todas las propiedades con detalles completos",
            Description = "Devuelve una lista de todas las propiedades con información detallada (tipo, venta, mejoras)."
        )]
        public async Task<IActionResult> GetAllWithInclude()
        {
            var response = await Mediator.Send(new GetAllPropertiesWithIncludeQuery());

            if (response == null || response.Count == 0)
            {
                return NoContent();
            }

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,DEVELOPER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtiene una propiedad por Id",
            Description = "Devuelve los datos de una propiedad específica utilizando su Id."
        )]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await Mediator.Send(new GetPropertyByIdQuery { Id = id });

            if (response == null)
            {
                return NotFound(new { message = $"No existe la propiedad con el Id {id}" });
            }

            return Ok(response);
        }

        [HttpGet("code/{code}")]
        [Authorize(Roles = "ADMIN,DEVELOPER")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Obtiene una propiedad por Código",
            Description = "Devuelve los datos de una propiedad específica utilizando su código único de 6 caracteres."
        )]
        public async Task<IActionResult> GetByCode([FromRoute] string code)
        {
            var response = await Mediator.Send(new GetPropertyByCodeQuery { Code = code });

            if (response == null)
            {
                return NotFound(new { message = $"No existe la propiedad con el código {code}" });
            }
            return Ok(response);
        }


    }
}
