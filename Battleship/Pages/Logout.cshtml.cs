using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Battleship.Pages
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
            // Session löschen
            HttpContext.Session.Clear();

            // Zur Startseite weiterleiten
            Response.Redirect("Index");
        }
    }
}
