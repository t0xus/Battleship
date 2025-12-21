using Battleship.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Pages
{
    public class AvailableGames
    {         
        public int MatchId { get; set; }
        public string MatchName { get; set; }
        public string HostUsername { get; set; }
    }

    public class BattleShips
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Boolean across { get; set; }
        public int boxes { get; set; }
    }

    public class LobbyModel : PageModel
    {

        public List<AvailableGames> AvailableGames { get; set; } = new List<AvailableGames>();
        public List<BattleShips> BattleShips { get; set; } = new List<BattleShips>()
        {
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 5 }, // Carrier
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 4 }, // Battleship
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 4 }, // Battleship
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 3 }, // Destroyer
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 3 }, // Destroyer
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 3 }, // Destroyer
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 2 },  // Submarine
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 2 },  // Submarine
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 2 },  // Submarine
            new BattleShips() { X = 0, Y = 0, across = true, boxes = 2 }  // Submarine
        };

        private readonly ILogger<LobbyModel> _logger;

        private readonly battleshipContext _context;
        [BindProperty]
        public string HostGame { get; set; }

        public string JoinMatchId { get; set;}

        public LobbyModel(ILogger<LobbyModel> logger, battleshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                Response.Redirect("Index");
            }

            // Lade verfügbare Spiele
            var matches = from m in _context.Matches
                          where m.IdParticipant == null && m.Done == false
                          && m.IdHost != HttpContext.Session.GetInt32("UserId")
                          join u in _context.Users on m.IdHost equals u.Id
                          select new AvailableGames
                          {
                              MatchId = m.Id,
                              MatchName = m.MatchName,
                              HostUsername = u.Username
                          };
            AvailableGames = matches.ToList();


        }

        public async Task<IActionResult> OnPost()
        {
            if(!string.IsNullOrEmpty(HostGame))
            {
                //Hole den eingelogten user
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = await _context.Users.FindAsync(userId);

                //Erstelle ein neues Spiel
                var match = new Match();
                match.MatchName = HostGame;
                match.IdHost = user.Id;
                match.IdParticipant = null;
                match.Done = false;
                _context.Matches.Add(match);
                await _context.SaveChangesAsync();

                OnGet();
                return Page();

            }
            if(!string.IsNullOrEmpty(JoinMatchId))
            {
                //Hole den eingelogten user
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = await _context.Users.FindAsync(userId);

                //Finde das Spiel
                int matchId = int.Parse(JoinMatchId);
                var match = await _context.Matches.FindAsync(matchId);
                if(match != null && match.IdParticipant == null && match.Done == false)
                {
                    match.IdParticipant = user.Id;
                    _context.Matches.Update(match);
                    await _context.SaveChangesAsync();
                }

                //Eine zufällige Zahl zwischen 0 und 1 generieren
                System.Random rand = new System.Random();
                int rand_turn = rand.Next(0, 2);
                if(rand_turn == 0)
                {
                    match.TurnHost = false;
                }
                else
                {
                    match.TurnHost = true;
                }

                _context.Matches.Update(match);
                await _context.SaveChangesAsync();

                createBattleShips();

            }

            OnGet();
            return Page();
        }

        private void createBattleShips()
        {
            System.Random rand = new System.Random();

            for (int i = 0; i < BattleShips.Count; i++)
            {
                BattleShips[i].X = rand.Next(1, 11);
                BattleShips[i].Y = rand.Next(1, 11);

                //Zufällig ausrichten
                int across = rand.Next(0, 2);
                if (across == 0)
                {
                    BattleShips[i].across = false;
                }
                else
                {
                    BattleShips[i].across = true;
                }

                for (int j = 0; j < i; j++)
                {
                    
                }
            }
        }
    }
}
