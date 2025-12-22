using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.Features.PropertyType.Commands.Create;
using RealStateApp.Core.Application.Features.PropertyType.Commands.Delete;
using RealStateApp.Core.Application.Features.PropertyType.Commands.Edit;
using RealStateApp.Core.Application.Features.PropertyType.Queries.GetAllWithInclude;
using RealStateApp.Core.Application.Features.PropertyType.Queries.GetById;
using Swashbuckle.AspNetCore.Annotations;

namespace RealStateWebApi.Controllers.v1
{
    /// <summary>
    /// Controlador para gestionar PropertyTypes
    /// CRUD completo: Crear, Obtener, Editar, Eliminar
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [SwaggerTag("CRUD operations for PropertyTypes")]
    public class PropertyTypesController : BaseApiController
    {
        /// <summary>
        /// Crea un nuevo PropertyType
        /// </summary>
        /// <param name="command">Datos del tipo de propiedad a crear</param>
        /// <returns>Id del PropertyType creado</returns>
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Crea un nuevo PropertyType",
            Description = "Crea un nuevo PropertyType y devuelve su Id")] 
        public async Task<IActionResult> Create([FromBody] CreatePropertyTypeCommand command)
        {
            var createdId = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = createdId }, createdId);
        }

        /// <summary>
        /// Actualiza un PropertyType existente
        /// </summary>
        /// <param name="id">Id del PropertyType</param>
        /// <param name="command">Datos actualizados</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Actualiza un PropertyType existente",
            Description = "Actualiza un PropertyType existente con los datos proporcionados")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] EditPropertyTypeCommand command)
        {
            if (id != command.Id)
                return BadRequest("El ID en la URL no coincide con el del cuerpo de la solicitud.");

            await Mediator.Send(command);
            return Ok(new { message = "PropertyType actualizado correctamente" });
        }

        /// <summary>
        /// Obtiene todos los tipos de propiedades  
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Obtiene todos los tipos de propiedades",
            Description = "Devuelve una lista de todos los tipos de propiedades disponibles")]
        public async Task<IActionResult> GetAll()
        {
            var response = await Mediator.Send(new GetAllPropertyTypeWithIncludeQuery());
            if (response == null || response.Count == 0)
                return NoContent();

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un tipo de propiedad  por su Id
        /// </summary>
        /// <param name="id">Id del PropertyType</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Obtiene un tipo de propiedad por su Id",
            Description = "Devuelve los detalles de un tipo de propiedad específico utilizando su Id")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var response = await Mediator.Send(new GetPropertyTypeByIdQuery { Id = id });
            if (response == null)
                return NoContent();

            return Ok(response);
        }

        /// <summary>
        /// Elimina un tipo de propiedad por Id
        /// </summary>
        /// <param name="id">Id del PropertyType</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Elimina un tipo de propiedad por Id",
            Description = "Elimina un tipo de propiedad específico utilizando su Id")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await Mediator.Send(new DeletePropertyTypeCommand { Id = id });
            return NoContent();
        }
    }
}
