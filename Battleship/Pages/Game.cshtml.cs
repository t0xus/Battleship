using Battleship.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Battleship.Pages
{
    public class GameModel : PageModel
    {
        private readonly battleshipContext _context;
        public GameModel(battleshipContext context) => _context = context;

        // --- Public View State ---
        public string ErrorMessage { get; private set; } = "";
        public string InfoMessage { get; private set; } = "";

        public int MatchId { get; private set; }
        public int UserId { get; private set; }
        public bool IsHost { get; private set; }
        public bool IsMyTurn { get; private set; }
        //public bool WaitForparticipant { get; private set; } = false;

        public static readonly string[] XLetters = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

        // Boards: key = y(1..10) -> entity row
        private Dictionary<int, Battleground> _ownRows = new();
        private Dictionary<int, Battleground> _enemyRows = new();

        // Own ships (für Umrandung)
        private List<ShipsBattleground> _ownShipParts = new();
        private Dictionary<(int x, int y), int> _ownCellToShipPartId = new();
        private Dictionary<int, HashSet<(int x, int y)>> _ownShipCellsByPart = new();

        // Enemy sunk ships only (für Umrandung nur wenn versenkt)
        private Dictionary<(int x, int y), int> _enemySunkCellToShipPartId = new();
        private Dictionary<int, HashSet<(int x, int y)>> _enemySunkShipCellsByPart = new();

        // POST binding
        [BindProperty] public int X { get; set; }  // 1..10
        [BindProperty] public int Y { get; set; }  // 1..10

        // ------------- GET -------------
        public async Task<IActionResult> OnGetAsync()
        {
            if (gameOver() == 1)
            {
                TempData["Message"] = "Spiel vorbei! Du hast gewonnen!";
                Response.Redirect("/Lobby");
            }
            else if (gameOver() == 2)
            {
                TempData["Message"] = "Spiel vorbei! Du hast verloren!";
                Response.Redirect("/Lobby");
            }

            if (!await LoadGameStateAsync())
                return Page();

            return Page();
        }

        // ------------- POST (Shoot) -------------
        public async Task<IActionResult> OnPostAsync()
        {
            if (!await LoadGameStateAsync())
                return Page();

            // Validierung
            if (X < 1 || X > 10 || Y < 1 || Y > 10)
            {
                ErrorMessage = "Ungültige Koordinate.";
                return Page();
            }

            if (!IsMyTurn)
            {
                ErrorMessage = "Du bist gerade nicht dran.";
                return Page();
            }

            // Schuss auf Gegnerfeld
            var enemyRow = _enemyRows.TryGetValue(Y, out var row) ? row : null;
            if (enemyRow == null)
            {
                ErrorMessage = "Gegnerfeld konnte nicht geladen werden.";
                return Page();
            }

            int current = GetCellValue(enemyRow, X) ?? 0;

            if (current == 1 || current == 3)
            {
                InfoMessage = "Auf dieses Feld wurde bereits geschossen.";
                return Page();
            }

            if (current == 2)
            {
                SetCellValue(enemyRow, X, 3);
                InfoMessage = $"Treffer auf {XLetters[X - 1]}{Y}!";
            }
            else if (current == 0 || current == null)
            {
                SetCellValue(enemyRow, X, 1);
                InfoMessage = $"Fehlschuss auf {XLetters[X - 1]}{Y}.";
            }

            // Turn wechseln (Host<->Participant)
            var match = await _context.Matches.FirstAsync(m => m.Id == MatchId);
            match.TurnHost = !(match.TurnHost ?? true);
            _context.Battlegrounds.Update(enemyRow);
            await _context.SaveChangesAsync();

            if (gameOver() == 1)
            {
                TempData["Message"] = "Spiel vorbei! Du hast gewonnen!";
                Response.Redirect("/Lobby");
            }
            else if (gameOver() == 2)
            {
                TempData["Message"] = "Spiel vorbei! Du hast verloren!";
                Response.Redirect("/Lobby");
            }

            // Reload state (damit versenkte Schiffe ggf. sichtbar werden)
            await LoadGameStateAsync();

            return Page();
        }

        // ------------- Rendering Helpers -------------
        public int GetOwnCell(int x, int y)
        {
            if (!_ownRows.TryGetValue(y, out var row)) return 0;
            return GetCellValue(row, x) ?? 0;
        }

        public int GetEnemyCell(int x, int y)
        {
            if (!_enemyRows.TryGetValue(y, out var row)) return 0;
            return GetCellValue(row, x) ?? 0;
        }

        // Own cell css: zeigt Schiffe immer (2/3), Miss (1), Hit (3)
        public string OwnCellCss(int x, int y)
        {
            int v = GetOwnCell(x, y);
            var cls = "cell";

            if (v == 1) cls += " miss";
            if (v == 2) cls += " ship";
            if (v == 3) cls += " hit ship"; // getroffener Schiffsteil bleibt Schiff

            // Borders pro Schiff (damit nahe Schiffe nicht „zusammenkleben“)
            if (_ownCellToShipPartId.TryGetValue((x, y), out var partId) &&
                _ownShipCellsByPart.TryGetValue(partId, out var shipCells))
            {
                bool left = shipCells.Contains((x - 1, y));
                bool right = shipCells.Contains((x + 1, y));
                bool up = shipCells.Contains((x, y - 1));
                bool down = shipCells.Contains((x, y + 1));

                if (!left) cls += " bL";
                if (!right) cls += " bR";
                if (!up) cls += " bT";
                if (!down) cls += " bB";
            }

            return cls;
        }

        // Enemy cell css:
        // - miss/hit always visible
        // - ships/borders ONLY when sunk
        public string EnemyCellCss(int x, int y)
        {
            int v = GetEnemyCell(x, y);
            var cls = "cell";

            if (v == 1) cls += " miss";
            if (v == 3) cls += " hit";

            // only show ship when sunk
            if (_enemySunkCellToShipPartId.TryGetValue((x, y), out var partId) &&
                _enemySunkShipCellsByPart.TryGetValue(partId, out var shipCells))
            {
                cls += " ship";

                bool left = shipCells.Contains((x - 1, y));
                bool right = shipCells.Contains((x + 1, y));
                bool up = shipCells.Contains((x, y - 1));
                bool down = shipCells.Contains((x, y + 1));

                if (!left) cls += " bL";
                if (!right) cls += " bR";
                if (!up) cls += " bT";
                if (!down) cls += " bB";
            }

            return cls;
        }

        public bool EnemyCellDisabled(int x, int y)
        {
            if (!IsMyTurn) return true;
            int v = GetEnemyCell(x, y);
            // bereits beschossen => disabled
            return (v == 1 || v == 3);
        }

        // ------------- Core State Loader -------------
        private async Task<bool> LoadGameStateAsync()
        {
            ErrorMessage = "";
            InfoMessage = "";

            UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (UserId <= 0)
            {
                ErrorMessage = "Nicht eingeloggt (Session UserId fehlt).";
                return false;
            }

            //var match = await _context.Matches.AsNoTracking().FirstOrDefaultAsync(m => m.Done == false);
            var match = await _context.Matches.AsNoTracking().FirstOrDefaultAsync(m => (m.Done == false) && (m.IdParticipant == UserId || m.IdHost == UserId));
            if (match == null)
            {
                ErrorMessage = "Kein aktives Match gefunden (Done=false).";
                return false;
            }

            MatchId = match.Id;

            IsHost = match.IdHost == UserId;
            bool isParticipant = match.IdParticipant == UserId;

            if (!IsHost && !isParticipant)
            {
                ErrorMessage = "Du gehörst nicht zu diesem Match (weder Host noch Participant).";
                return false;
            }

            bool turnHost = match.TurnHost ?? true;
            IsMyTurn = IsHost ? turnHost : !turnHost;

            // Load own/enemy battleground rows
            bool ownFlag = IsHost;           // Host sieht FieldHost=true als eigenes Feld
            bool enemyFlag = !IsHost;        // Gegnerfeld ist jeweils invertiert

            var ownRows = await _context.Battlegrounds
                .Where(b => b.IdMatch == MatchId && b.FieldHost == ownFlag)
                .AsNoTracking()
                .ToListAsync();

            var enemyRows = await _context.Battlegrounds
                .Where(b => b.IdMatch == MatchId && b.FieldHost == enemyFlag)
                .AsNoTracking()
                .ToListAsync();

            _ownRows = ownRows.Where(r => r.YCord.HasValue).ToDictionary(r => r.YCord!.Value, r => r);
            _enemyRows = enemyRows.Where(r => r.YCord.HasValue).ToDictionary(r => r.YCord!.Value, r => r);

            // Load ship parts for own side (ship_host = ownFlag)
            _ownShipParts = await _context.ShipsBattlegrounds
                .Where(s => s.IdMatch == MatchId && s.ShipHost == ownFlag)
                .AsNoTracking()
                .ToListAsync();

            BuildShipIndexForSide(_ownShipParts, forOwnSide: true);

            // Enemy ships for sunk rendering
            var enemyParts = await _context.ShipsBattlegrounds
                .Where(s => s.IdMatch == MatchId && s.ShipHost == enemyFlag)
                .AsNoTracking()
                .ToListAsync();

            BuildEnemySunkShips(enemyParts);

            return true;
        }

        // ------------- Ship indexing (own) -------------
        private void BuildShipIndexForSide(List<ShipsBattleground> parts, bool forOwnSide)
        {
            _ownCellToShipPartId.Clear();
            _ownShipCellsByPart.Clear();

            foreach (var part in parts)
            {
                if (part.XCord == null || part.YCord == null) continue;

                var ship = _context.Ships.AsNoTracking().FirstOrDefault(s => s.Id == part.IdShip);
                if (ship == null) continue;
                int len = ship.Boxes ?? 0;
                if (len <= 0) continue;

                var cells = new HashSet<(int x, int y)>();
                for (int i = 0; i < len; i++)
                {
                    int x = (part.Across ?? false) ? part.XCord.Value + i : part.XCord.Value;
                    int y = (part.Across ?? false) ? part.YCord.Value : part.YCord.Value + i;
                    if (x < 1 || x > 10 || y < 1 || y > 10) { cells.Clear(); break; }
                    cells.Add((x, y));
                }
                if (cells.Count == 0) continue;

                int partId = part.Id;
                _ownShipCellsByPart[partId] = cells;
                foreach (var c in cells)
                    _ownCellToShipPartId[c] = partId;
            }
        }

        // ------------- Enemy sunk ships -------------
        private void BuildEnemySunkShips(List<ShipsBattleground> enemyShipParts)
        {
            _enemySunkCellToShipPartId.Clear();
            _enemySunkShipCellsByPart.Clear();

            // Ships table in one go
            var shipMap = _context.Ships.AsNoTracking().ToDictionary(s => s.Id, s => s);

            foreach (var part in enemyShipParts)
            {
                if (part.XCord == null || part.YCord == null) continue;
                if (!shipMap.TryGetValue(part.IdShip ?? 0, out var ship)) continue;

                int len = ship.Boxes ?? 0;
                if (len <= 0) continue;

                var cells = new HashSet<(int x, int y)>();
                for (int i = 0; i < len; i++)
                {
                    int x = (part.Across ?? false) ? part.XCord.Value + i : part.XCord.Value;
                    int y = (part.Across ?? false) ? part.YCord.Value : part.YCord.Value + i;
                    if (x < 1 || x > 10 || y < 1 || y > 10) { cells.Clear(); break; }
                    cells.Add((x, y));
                }
                if (cells.Count == 0) continue;

                bool sunk = cells.All(c => GetEnemyCell(c.x, c.y) == 3);
                if (!sunk) continue;

                int partId = part.Id;
                _enemySunkShipCellsByPart[partId] = cells;
                foreach (var c in cells)
                    _enemySunkCellToShipPartId[c] = partId;
            }
        }

        // ------------- Battleground Field Access -------------
        private static int? GetCellValue(Battleground row, int x)
        {
            return x switch
            {
                1 => row.FieldA,
                2 => row.FieldB,
                3 => row.FieldC,
                4 => row.FieldD,
                5 => row.FieldE,
                6 => row.FieldF,
                7 => row.FieldG,
                8 => row.FieldH,
                9 => row.FieldI,
                10 => row.FieldJ,
                _ => 0
            };
        }

        private static void SetCellValue(Battleground row, int x, int value)
        {
            switch (x)
            {
                case 1: row.FieldA = value; break;
                case 2: row.FieldB = value; break;
                case 3: row.FieldC = value; break;
                case 4: row.FieldD = value; break;
                case 5: row.FieldE = value; break;
                case 6: row.FieldF = value; break;
                case 7: row.FieldG = value; break;
                case 8: row.FieldH = value; break;
                case 9: row.FieldI = value; break;
                case 10: row.FieldJ = value; break;
            }
        }

        private int gameOver()
        {
            var match_row = _context.Matches
                .Where(m => (m.IdHost == HttpContext.Session.GetInt32("UserId") || m.IdParticipant == HttpContext.Session.GetInt32("UserId")) && m.Done == false)
                .FirstOrDefault();

            if (match_row == null)
            {
                return 2;
            }

            int count_hits_participant = 0;
            int count_hits_host = 0;

            for (int i = 1; i <= 10; i++)
            {
                //Hole Battleground row
                Battleground bg_row;

                bg_row = _context.Battlegrounds
                .Where(b => b.IdMatch == match_row.Id && b.FieldHost == false && b.YCord == i)
                .FirstOrDefault();

                if (bg_row == null) return 0;

                if (bg_row.FieldA == 3) count_hits_participant++;
                if (bg_row.FieldB == 3) count_hits_participant++;
                if (bg_row.FieldC == 3) count_hits_participant++;
                if (bg_row.FieldD == 3) count_hits_participant++;
                if (bg_row.FieldE == 3) count_hits_participant++;
                if (bg_row.FieldF == 3) count_hits_participant++;
                if (bg_row.FieldG == 3) count_hits_participant++;
                if (bg_row.FieldH == 3) count_hits_participant++;
                if (bg_row.FieldI == 3) count_hits_participant++;
                if (bg_row.FieldJ == 3) count_hits_participant++;
            }
            for (int i = 1; i <= 10; i++)
            {
                //Hole Battleground row
                Battleground bg_row;

                bg_row = _context.Battlegrounds
                .Where(b => b.IdMatch == match_row.Id && b.FieldHost == true && b.YCord == i)
                .FirstOrDefault();

                if (bg_row == null) return 0;

                if (bg_row.FieldA == 3) count_hits_host++;
                if (bg_row.FieldB == 3) count_hits_host++;
                if (bg_row.FieldC == 3) count_hits_host++;
                if (bg_row.FieldD == 3) count_hits_host++;
                if (bg_row.FieldE == 3) count_hits_host++;
                if (bg_row.FieldF == 3) count_hits_host++;
                if (bg_row.FieldG == 3) count_hits_host++;
                if (bg_row.FieldH == 3) count_hits_host++;
                if (bg_row.FieldI == 3) count_hits_host++;
                if (bg_row.FieldJ == 3) count_hits_host++;
            }

            if (count_hits_participant >= 30 && match_row.IdHost == HttpContext.Session.GetInt32("UserId"))
            {
                match_row.Done = true;
                _context.SaveChanges();
                return 1;
            }
            else if (count_hits_host >= 30 && match_row.IdParticipant == HttpContext.Session.GetInt32("UserId"))
            {
                match_row.Done = true;
                _context.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool WaitForparticipant()
        {
            var match = _context.Matches
                .Where(m => (m.IdHost == HttpContext.Session.GetInt32("UserId") || m.IdParticipant == HttpContext.Session.GetInt32("UserId")) && m.Done == false)
                .FirstOrDefault();

            if (match == null)
            {
                return false;
            }

            if (match.IdParticipant == null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
