using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.GLMS.API.DTOs;
using TechMove.GLMS.API.Services;

namespace TechMove.GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;

        public ContractsController(IContractService service)
        {
            _service = service;
        }

        // GET /api/contracts?status=Active
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var contracts = await _service.GetAllAsync(status);
            return Ok(contracts);
        }

        // GET /api/contracts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _service.GetByIdAsync(id);
            if (contract == null)
                return NotFound();

            return Ok(contract);
        }

        // POST /api/contracts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractCreateDto dto)
        {
            var contract = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        // PATCH /api/contracts/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] ContractUpdateStatusDto dto)
        {
            var updated = await _service.UpdateStatusAsync(id, dto.Status);

            if (!updated)
                return NotFound();

            return Ok(new { message = "Status updated successfully" });
        }
    }
}
