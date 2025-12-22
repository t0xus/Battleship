using Battleship.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        public List<BattleShips> BattleShipsHost { get; set; } = new List<BattleShips>()
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

        public List<BattleShips> BattleShipsParticipant { get; set; } = new List<BattleShips>()
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
        [BindProperty]
        public string JoinMatchId { get; set; }

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
            if (!string.IsNullOrEmpty(HostGame))
            {
                //Hole den eingelogten user
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = await _context.Users.FindAsync(userId);

                //Erstelle ein neues Spiel
                var match = new Battleship.Models.Match();
                match.MatchName = HostGame;
                match.IdHost = user.Id;
                match.IdParticipant = null;
                match.Done = false;
                _context.Matches.Add(match);
                await _context.SaveChangesAsync();

                OnGet();
                return Page();

            }
            if (!string.IsNullOrEmpty(JoinMatchId))
            {
                //Hole den eingelogten user
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = await _context.Users.FindAsync(userId);

                //Finde das Spiel
                int matchId = int.Parse(JoinMatchId);
                var match = await _context.Matches.FindAsync(matchId);
                if (match != null && match.IdParticipant == null && match.Done == false)
                {
                    match.IdParticipant = user.Id;
                    _context.Matches.Update(match);
                    await _context.SaveChangesAsync();
                }

                //Eine zufällige Zahl zwischen 0 und 1 generieren
                System.Random rand = new System.Random();
                int rand_turn = rand.Next(0, 2);
                if (rand_turn == 0)
                {
                    match.TurnHost = false;
                }
                else
                {
                    match.TurnHost = true;
                }

                _context.Matches.Update(match);
                await _context.SaveChangesAsync();

                BattleShipsHost = createBattleShips(BattleShipsHost);
                BattleShipsParticipant = createBattleShips(BattleShipsParticipant);
                
                saveBattleShips(BattleShipsHost, match.Id, true);
                saveBattleShips(BattleShipsParticipant, match.Id, false);
            }

            OnGet();
            return Page();
        }

        private List<BattleShips> createBattleShips(List<BattleShips> BattleShips)
        {
            System.Random rand = new System.Random();

            for (int i = 0; i < BattleShips.Count; i++)
            {

                do
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
                } while ((BattleShips[i].across && (BattleShips[i].X + BattleShips[i].boxes - 1 > 10)) ||
                         (!BattleShips[i].across && (BattleShips[i].Y + BattleShips[i].boxes - 1 > 10)));


                for (int j = 0; j < i; j++)
                {
                    for (int k = 0; k < BattleShips[i].boxes; k++)
                    {

                        int x1 = 0;
                        int y1 = 0;
                        int x2 = 0;
                        int y2 = 0;



                        for (int l = 0; l < BattleShips[j].boxes; l++)
                        {


                            if (BattleShips[i].across)
                            {
                                x1 = BattleShips[i].X + k;
                                y1 = BattleShips[i].Y;
                            }
                            else
                            {
                                x1 = BattleShips[i].X;
                                y1 = BattleShips[i].Y + k;
                            }

                            if (BattleShips[j].across)
                            {
                                x2 = BattleShips[j].X + l;
                                y2 = BattleShips[j].Y;
                            }
                            else
                            {
                                x2 = BattleShips[j].X;
                                y2 = BattleShips[j].Y + l;
                            }

                            if ((x1 == x2 && y1 == y2))
                            {
                                do
                                {
                                    //Kollision, neu generieren
                                    BattleShips[j].X = rand.Next(1, 11);
                                    BattleShips[j].Y = rand.Next(1, 11);

                                    //Zufällig ausrichten
                                    int across_new = rand.Next(0, 2);
                                    if (across_new == 0)
                                    {
                                        BattleShips[j].across = false;
                                    }
                                    else
                                    {
                                        BattleShips[j].across = true;
                                    }
                                } while ((BattleShips[j].across && (BattleShips[j].X + BattleShips[j].boxes - 1 > 10)) ||
                                         (!BattleShips[j].across && (BattleShips[j].Y + BattleShips[j].boxes - 1 > 10)));
                                //Setze j zurück, um erneut zu prüfen
                                j = -1;
                                break;
                            }
                        }

                        if (x1 == x2 && y1 == y2)
                        {
                            break;
                        }
                    }
                }
            }
        
            return BattleShips;
        }

        private void saveBattleShips(List<BattleShips> BattleShips, int match_id, Boolean field_host)
        {
            //Speichere die Schiffe in der Datenbank
            for (int i = 1; i <= 10; i++)
            {
                var battleground_raw = new Battleground();

                battleground_raw.IdMatch = match_id;
                battleground_raw.FieldHost = field_host;
                battleground_raw.YCord = i;
                battleground_raw.FieldA = 0;
                battleground_raw.FieldB = 0;
                battleground_raw.FieldC = 0;
                battleground_raw.FieldD = 0;
                battleground_raw.FieldE = 0;
                battleground_raw.FieldF = 0;
                battleground_raw.FieldG = 0;
                battleground_raw.FieldH = 0;
                battleground_raw.FieldI = 0;
                battleground_raw.FieldJ = 0;


                _context.Battlegrounds.Add(battleground_raw);
            }

            _context.SaveChanges();

            foreach (var ship in BattleShips)
            {
                if (ship.across)
                {
                    var battleground_line = _context.Battlegrounds.FirstOrDefault(b => b.IdMatch == match_id && b.YCord == ship.Y && b.FieldHost == field_host);

                    for (int i = 0; i < ship.boxes; i++)
                    {
                        switch (ship.X + i)
                        {
                            case 1:
                                battleground_line.FieldA = 2;
                                break;
                            case 2:
                                battleground_line.FieldB = 2;
                                break;
                            case 3:
                                battleground_line.FieldC = 2;
                                break;
                            case 4:
                                battleground_line.FieldD = 2;
                                break;
                            case 5:
                                battleground_line.FieldE = 2;
                                break;
                            case 6:
                                battleground_line.FieldF = 2;
                                break;
                            case 7:
                                battleground_line.FieldG = 2;
                                break;
                            case 8:
                                battleground_line.FieldH = 2;
                                break;
                            case 9:
                                battleground_line.FieldI = 2;
                                break;
                            case 10:
                                battleground_line.FieldJ = 2;
                                break;
                        }
                    }

                    _context.Battlegrounds.Update(battleground_line);

                }
                else
                {
                    for (int i = ship.Y; i < ship.Y + ship.boxes; i++)
                    {
                        var battleground_line = _context.Battlegrounds.FirstOrDefault(b => b.IdMatch == match_id && b.YCord == i && b.FieldHost == field_host);

                        switch (ship.X)
                        {
                            case 1:
                                battleground_line.FieldA = 2;
                                break;
                            case 2:
                                battleground_line.FieldB = 2;
                                break;
                            case 3:
                                battleground_line.FieldC = 2;
                                break;
                            case 4:
                                battleground_line.FieldD = 2;
                                break;
                            case 5:
                                battleground_line.FieldE = 2;
                                break;
                            case 6:
                                battleground_line.FieldF = 2;
                                break;
                            case 7:
                                battleground_line.FieldG = 2;
                                break;
                            case 8:
                                battleground_line.FieldH = 2;
                                break;
                            case 9:
                                battleground_line.FieldI = 2;
                                break;
                            case 10:
                                battleground_line.FieldJ = 2;
                                break;
                        }

                        _context.Battlegrounds.Update(battleground_line);
                    }
                }

                _context.SaveChanges();

            }
        }
    }
}
