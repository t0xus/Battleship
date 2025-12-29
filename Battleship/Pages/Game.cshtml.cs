using Battleship.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Battleship.Pages
{
    public class GameModel : PageModel
    {

        private readonly ILogger<GameModel> _logger;

        private readonly battleshipContext _context;
        //public GameModel(battleshipContext context) => _context = context;

        [BindProperty(SupportsGet = true)]
        public int IdMatch { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Host { get; set; }

        public string[] Columns { get; } = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

        // 10x10 Feld (Index 1..10)
        private int[,] _grid = new int[11, 11];

        // Für Umrandung: Zellen pro ShipId
        private Dictionary<int, HashSet<(int x, int y)>> _shipCellsByPlacementId = new();



        public GameModel(ILogger<GameModel> logger, battleshipContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            await LoadBoardAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadBoardAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostShootAsync(string shot)
        {
            // shot = "x,y"
            var parts = shot.Split(',');
            if (parts.Length != 2
                || !int.TryParse(parts[0], out var x)
                || !int.TryParse(parts[1], out var y)
                || x < 1 || x > 10 || y < 1 || y > 10)
            {
                await LoadBoardAsync();
                return Page();
            }

            // Battleground-Zeile für (IdMatch, Host, y)
            var row = await _context.Battlegrounds
                .FirstOrDefaultAsync(b => b.IdMatch == IdMatch && b.FieldHost == Host && b.YCord == y);

            if (row == null)
            {
                await LoadBoardAsync();
                return Page();
            }

            // Feld-Value lesen und updaten
            int current = GetFieldValue(row, x);
            if (current == 2) SetFieldValue(row, x, 3);      // Treffer
            else if (current == 0) SetFieldValue(row, x, 1); // Fehlschuss

            await _context.SaveChangesAsync();

            // Neu laden fürs UI
            await LoadBoardAsync();
            return Page();
        }

        // ---------- Public helpers for cshtml ----------

        public int GetCell(int x, int y) => _grid[x, y];

        public string GetCellCss(int x, int y)
        {
            // Grundzustand aus Battleground
            var v = _grid[x, y];
            var cls = v switch
            {
                0 => "miss",
                1 => "hitbad",
                2 => "ship",
                3 => "shiphit",
                _ => ""
            };

            // Umrandung nur für Schiffszellen (2/3)
            if (v is 2 or 3)
            {
                cls += " " + BorderClassForCell(x, y);
            }

            return cls.Trim();
        }

        // ---------- Load board + ships ----------

        private async Task LoadBoardAsync()
        {
            // 1) Matrix laden
            Array.Clear(_grid, 0, _grid.Length);

            var rows = await _context.Battlegrounds
                .Where(b => b.IdMatch == IdMatch && b.FieldHost == Host)
                .OrderBy(b => b.YCord)
                .ToListAsync();

            foreach (var r in rows)
            {
                int y = r.YCord ?? 0;
                if (y < 1 || y > 10) continue;

                _grid[1, y] = r.FieldA ?? 0;
                _grid[2, y] = r.FieldB ?? 0;
                _grid[3, y] = r.FieldC ?? 0;
                _grid[4, y] = r.FieldD ?? 0;
                _grid[5, y] = r.FieldE ?? 0;
                _grid[6, y] = r.FieldF ?? 0;
                _grid[7, y] = r.FieldG ?? 0;
                _grid[8, y] = r.FieldH ?? 0;
                _grid[9, y] = r.FieldI ?? 0;
                _grid[10, y] = r.FieldJ ?? 0;
            }

            // 2) ShipsBattlegrounds + Ship laden (für Boxes) – WICHTIG: pro Placement (= ships_battleground.id)
            var placements = await _context.ShipsBattlegrounds
                .Where(s => s.IdMatch == IdMatch && s.ShipHost == Host)
                .ToListAsync();

            // shipId (Typ) -> boxes
            var shipTypeIds = placements
                .Where(p => p.IdShip != null)
                .Select(p => p.IdShip!.Value)
                .Distinct()
                .ToList();

            var shipTypes = await _context.Ships
                .Where(s => shipTypeIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Boxes);

            _shipCellsByPlacementId = new Dictionary<int, HashSet<(int x, int y)>>();

            foreach (var p in placements)
            {
                if (p.Id == 0) continue; // Placement-PK muss existieren
                if (p.IdShip == null || p.XCord == null || p.YCord == null || p.Across == null) continue;
                if (!shipTypes.TryGetValue(p.IdShip.Value, out var boxes)) continue;

                if (!_shipCellsByPlacementId.TryGetValue(p.Id, out var set))
                    _shipCellsByPlacementId[p.Id] = set = new HashSet<(int x, int y)>();

                for (int i = 0; i < boxes; i++)
                {
                    int x = p.Across.Value ? p.XCord.Value + i : p.XCord.Value;
                    int y = p.Across.Value ? p.YCord.Value : p.YCord.Value + i;

                    if (x is >= 1 and <= 10 && y is >= 1 and <= 10)
                        set.Add((x, y));
                }
            }
        }

        // ---------- Border logic (PRO shipId) ----------

        private string BorderClassForCell(int x, int y)
        {
            foreach (var kv in _shipCellsByPlacementId)
            {
                var cells = kv.Value;
                if (!cells.Contains((x, y))) continue;

                bool left = cells.Contains((x - 1, y));
                bool right = cells.Contains((x + 1, y));
                bool top = cells.Contains((x, y - 1));
                bool bottom = cells.Contains((x, y + 1));

                var cls = "";
                if (!top) cls += " bt";
                if (!right) cls += " br";
                if (!bottom) cls += " bb";
                if (!left) cls += " bl";
                return cls.Trim();
            }
            return "";
        }

        // ---------- Battleground field helpers ----------

        private static int GetFieldValue(dynamic row, int x) => x switch
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

        private static void SetFieldValue(dynamic row, int x, int val)
        {
            switch (x)
            {
                case 1: row.FieldA = val; break;
                case 2: row.FieldB = val; break;
                case 3: row.FieldC = val; break;
                case 4: row.FieldD = val; break;
                case 5: row.FieldE = val; break;
                case 6: row.FieldF = val; break;
                case 7: row.FieldG = val; break;
                case 8: row.FieldH = val; break;
                case 9: row.FieldI = val; break;
                case 10: row.FieldJ = val; break;
            }
        }
    }
}
