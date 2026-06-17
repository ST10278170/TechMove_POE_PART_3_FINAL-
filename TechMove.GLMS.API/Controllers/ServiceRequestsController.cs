using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Services;

namespace TechMove.GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _service;

        public ServiceRequestsController(IServiceRequestService service)
        {
            _service = service;
        }

        // GET /api/servicerequests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _service.GetAllAsync();
            return Ok(requests);
        }

        // GET /api/servicerequests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var request = await _service.GetByIdAsync(id);

            if (request == null)
                return NotFound();

            return Ok(request);
        }

        // POST /api/servicerequests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequestCreateDto dto)
        {
            try
            {
                var request = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/servicerequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceRequestUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);

            if (!updated)
                return NotFound();

            return Ok(new { message = "Service request updated successfully" });
        }

        // DELETE /api/servicerequests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { message = "Service request deleted successfully" });
        }
    }
}
