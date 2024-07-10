using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.AspNetCore.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace DXApplicationCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View();
        }

        public IActionResult RichEditor()
        {
            return View();
        }

        public IActionResult Spreadsheet()
        {
            return View();
        }

        [HttpPost]
        [HttpGet]
        public IActionResult DxDocumentRequest()
        {
            return SpreadsheetRequestProcessor.GetResponse(HttpContext);
        }
    }
}
