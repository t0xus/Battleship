using Battleship.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace Battleship.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        private readonly ILogger<IndexModel> _logger;

        private readonly battleshipContext _context;

        public Boolean wrong_credentials = false;

        public IndexModel(ILogger<IndexModel> logger, battleshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                // Benutzer ist bereits eingeloggt, weiterleiten zur Lobby
                Response.Redirect("/Lobby");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            string hashed_pw = ComputeSha256Hash(Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == Username && u.PwHash == hashed_pw);

            if (user != null)
            {                 // Benutzer gefunden, Session setzen
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);

                return RedirectToPage("/Lobby");
            }
            else
            {
                // Benutzer nicht gefunden, Fehlermeldung anzeigen
                ModelState.AddModelError(string.Empty, "Ungültiger Benutzername oder Passwort.");

                wrong_credentials = true;

                return Page();
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - gibt ein Byte-Array zurück
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Byte-Array in einen hexadezimalen String umwandeln
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
