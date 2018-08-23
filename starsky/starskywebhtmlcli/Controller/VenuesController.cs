using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using starsky.Models;
using starsky.Services;

namespace starskywebhtmlcli.Controller
{
    public class VenuesController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IViewRenderService _viewRender;

        public VenuesController(IViewRenderService viewRender)
        {
            _viewRender = viewRender;
        }

        public async Task<IActionResult> Edit(List<FileIndexItem> list)
        {
            string html = await _viewRender.RenderToStringAsync("Shared/Autopost", list);
            return Ok();
        }
    }
}