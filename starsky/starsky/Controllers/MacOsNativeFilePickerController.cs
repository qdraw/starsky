using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.native.FileSystem.Interfaces;
using starsky.foundation.native.FileSystem.Models;

namespace starsky.Controllers;

[ApiController]
[Authorize]
[Route("api/native/file-picker")]
public class MacOsNativeFilePickerController : ControllerBase
{
	private readonly IMacOsNativeFilePicker _picker;

	public MacOsNativeFilePickerController(IMacOsNativeFilePicker picker)
	{
		_picker = picker;
	}

	[HttpPost("folder")]
	public ActionResult<MacOsFolderPickResult> PickFolder([FromQuery] bool includeFiles = false)
	{
		var result = _picker.TryPickFolder(includeFiles);
		return Ok(result);
	}
}

