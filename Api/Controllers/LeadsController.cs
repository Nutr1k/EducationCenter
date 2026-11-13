using EducationCenter.DTO;
using EducationCenter.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace EducationCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
	private readonly ILeadService _leadService;

	public LeadsController(ILeadService leadService)
	{
		_leadService = leadService;
	}

	[HttpPost]
	[HttpPost]
	public async Task<IActionResult> Create(CreateLeadRequest request, CancellationToken cancellationToken)
	{
		await _leadService.CreateAsync(request, cancellationToken);

		return Ok();
	}
}