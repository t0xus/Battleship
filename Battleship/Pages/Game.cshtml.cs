using Battleship.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Battleship.Pages
{
    public class GameModel : PageModel
    {

        private readonly ILogger<GameModel> _logger;

        private readonly battleshipContext _context;

        public List<List<int?>> OwnMap { get; set; } = new List<List<int?>>();
        public List<List<int?>> OpponentMap { get; set; } = new List<List<int?>>();

        public Boolean myTurn = false;

        [BindProperty]
        public string y_cord { get; set; }

        [BindProperty]
        public string x_cord { get; set; }

        public GameModel(ILogger<GameModel> logger, battleshipContext context)
        {
            _logger = logger;
            _context = context;

            //Initialisiere eigene Map und Gegner Map mit 0
            for (int i = 0; i < 10; i++)
            {
                OwnMap.Add(new List<int?>());
                OpponentMap.Add(new List<int?>());
                for (int j = 0; j < 10; j++)
                {
                    OwnMap[i].Add(0);
                    OpponentMap[i].Add(0);
                }
            }
        }

        public void OnGet()
        {
            //Hole match_row 
            var match_row = _context.Matches
                .Where(m => (m.IdHost == HttpContext.Session.GetInt32("UserId") || m.IdParticipant == HttpContext.Session.GetInt32("UserId")) && m.Done == false)
                .FirstOrDefault();

            if (match_row == null)
            {
                //Kein aktives Match, weiterleiten zur Lobby
                TempData["Message"] = "Game Over!";
                Response.Redirect("/Lobby");
            }
            else if(match_row.IdParticipant == null)
            {                 
                //Gegner ist noch nicht beigetreten
                TempData["Message"] = "Opponent has not joined yet. Please wait!";
                //Response.Redirect("/Game");
            }
            else if (gameOver(match_row))
            {
                //Spiel vorbei, weiterleiten zur Lobby
                TempData["Message"] = "Game Over!";
                Response.Redirect("/Lobby");
            }
            else
            { 
                if (match_row.IdParticipant == HttpContext.Session.GetInt32("UserId"))
                {
                    if (match_row.TurnHost == false)
                    {
                        myTurn = true;
                    }
                    else
                    {
                        myTurn = false;
                    }
                }
                else
                {
                    if (match_row.TurnHost == true)
                    {
                        myTurn = true;
                    }
                    else
                    {
                        myTurn = false;
                    }
                }


                    for (int i = 1; i <= 10; i++) {

                    //Hole Battleground row
                    Battleground bg_row;

                    if (match_row.IdParticipant == HttpContext.Session.GetInt32("UserId"))
                    {
                        bg_row = _context.Battlegrounds
                        .Where(b => b.IdMatch == match_row.Id && b.FieldHost == false && b.YCord == i)
                        .FirstOrDefault();
                    }
                    else
                    {
                        bg_row = _context.Battlegrounds
                        .Where(b => b.IdMatch == match_row.Id && b.FieldHost == true && b.YCord == i)
                        .FirstOrDefault();
                    }

                    OwnMap[i - 1][0] = bg_row.FieldA;
                    OwnMap[i - 1][1] = bg_row.FieldB;
                    OwnMap[i - 1][2] = bg_row.FieldC;
                    OwnMap[i - 1][3] = bg_row.FieldD;
                    OwnMap[i - 1][4] = bg_row.FieldE;
                    OwnMap[i - 1][5] = bg_row.FieldF;
                    OwnMap[i - 1][6] = bg_row.FieldG;
                    OwnMap[i - 1][7] = bg_row.FieldH;
                    OwnMap[i - 1][8] = bg_row.FieldI;
                    OwnMap[i - 1][9] = bg_row.FieldJ;

                }

                for (int i = 1; i <= 10; i++)
                {

                    //Hole Battleground row
                    Battleground bg_row;

                    if (match_row.IdParticipant == HttpContext.Session.GetInt32("UserId"))
                    {
                        bg_row = _context.Battlegrounds
                        .Where(b => b.IdMatch == match_row.Id && b.FieldHost == true && b.YCord == i)
                        .FirstOrDefault();
                    }
                    else
                    {
                        bg_row = _context.Battlegrounds
                        .Where(b => b.IdMatch == match_row.Id && b.FieldHost == false && b.YCord == i)
                        .FirstOrDefault();
                    }

                    OpponentMap[i - 1][0] = bg_row.FieldA;
                    OpponentMap[i - 1][1] = bg_row.FieldB;
                    OpponentMap[i - 1][2] = bg_row.FieldC;
                    OpponentMap[i - 1][3] = bg_row.FieldD;
                    OpponentMap[i - 1][4] = bg_row.FieldE;
                    OpponentMap[i - 1][5] = bg_row.FieldF;
                    OpponentMap[i - 1][6] = bg_row.FieldG;
                    OpponentMap[i - 1][7] = bg_row.FieldH;
                    OpponentMap[i - 1][8] = bg_row.FieldI;
                    OpponentMap[i - 1][9] = bg_row.FieldJ;

                    if (bg_row.FieldA == 2)
                    {
                        OpponentMap[i - 1][0] = 0;
                    }
                    if (bg_row.FieldB == 2)
                    {
                        OpponentMap[i - 1][1] = 0;
                    }
                    if (bg_row.FieldC == 2)
                    {
                        OpponentMap[i - 1][2] = 0;
                    }
                    if (bg_row.FieldD == 2)
                    {
                        OpponentMap[i - 1][3] = 0;
                    }
                    if (bg_row.FieldE == 2)
                    {
                        OpponentMap[i - 1][4] = 0;
                    }
                    if (bg_row.FieldF == 2)
                    {
                        OpponentMap[i - 1][5] = 0;
                    }
                    if (bg_row.FieldG == 2)
                    {
                        OpponentMap[i - 1][6] = 0;
                    }
                    if (bg_row.FieldH == 2)
                    {
                        OpponentMap[i - 1][7] = 0;
                    }
                    if (bg_row.FieldI == 2)
                    {
                        OpponentMap[i - 1][8] = 0;
                    }
                    if (bg_row.FieldJ == 2)
                    {
                        OpponentMap[i - 1][9] = 0;
                    }
                }
            }

        }

        public async Task<IActionResult> OnPost()
        {
            if(!string.IsNullOrEmpty(x_cord) || !string.IsNullOrEmpty(y_cord))
            {
                
                //Verarbeite Schuss
                var match_row = _context.Matches
                    .Where(m => (m.IdHost == HttpContext.Session.GetInt32("UserId") || m.IdParticipant == HttpContext.Session.GetInt32("UserId")) && m.Done == false)
                    .FirstOrDefault();

                if (match_row != null)
                {
                    if (match_row.TurnHost == true)
                    {
                        match_row.TurnHost = false;
                    }
                    else
                    {
                        match_row.TurnHost = true;
                    }

                }



                //Hole Battleground row
                Battleground bg_row;

                if (match_row.IdParticipant == HttpContext.Session.GetInt32("UserId"))
                {
                    bg_row = _context.Battlegrounds
                    .Where(b => b.IdMatch == match_row.Id && b.FieldHost == true && b.YCord == Convert.ToInt32(y_cord)
                    ).FirstOrDefault();
                }
                else
                {
                    bg_row = _context.Battlegrounds
                    .Where(b => b.IdMatch == match_row.Id && b.FieldHost == false && b.YCord == Convert.ToInt32(y_cord)
                    ).FirstOrDefault();
                }

                int x = Convert.ToInt32(x_cord);

                switch (x)
                {
                    case 1:
                        if (bg_row.FieldA == 2)
                        {
                            bg_row.FieldA = 3; //Treffer
                        }
                        else if (bg_row.FieldA == 0)
                        {
                            bg_row.FieldA = 1; //Wasser
                        }
                        break;
                    case 2:
                        if (bg_row.FieldB == 2)
                        {
                            bg_row.FieldB = 3;
                        }
                        else if (bg_row.FieldB == 0)
                        {
                            bg_row.FieldB = 1;
                        }
                        break;
                    case 3:
                        if (bg_row.FieldC == 2)
                        {
                            bg_row.FieldC = 3;
                        }
                        else if (bg_row.FieldC == 0)
                        {
                            bg_row.FieldC = 1;
                        }
                        break;
                    case 4:
                        if (bg_row.FieldD == 2)
                        {
                            bg_row.FieldD = 3;
                        }
                        else if (bg_row.FieldD == 0)
                        {
                            bg_row.FieldD = 1;
                        }
                        break;
                    case 5:
                        if (bg_row.FieldE == 2)
                        {
                            bg_row.FieldE = 3;
                        }
                        else if (bg_row.FieldE == 0)
                        {
                            bg_row.FieldE = 1;
                        }
                        break;
                    case 6:
                        if (bg_row.FieldF == 2)
                        {
                            bg_row.FieldF = 3;
                        }
                        else if (bg_row.FieldF == 0)
                        {
                            bg_row.FieldF = 1;
                        }
                        break;
                    case 7:
                        if (bg_row.FieldG == 2)
                        {
                            bg_row.FieldG = 3;
                        }
                        else if (bg_row.FieldG == 0)
                        {
                            bg_row.FieldG = 1;
                        }
                        break;
                    case 8:
                        if (bg_row.FieldH == 2)
                        {
                            bg_row.FieldH = 3;
                        }
                        else if (bg_row.FieldH == 0)
                        {
                            bg_row.FieldH = 1;
                        }
                        break;
                    case 9:
                        if (bg_row.FieldI == 2)
                        {
                            bg_row.FieldI = 3;
                        }
                        else if (bg_row.FieldI == 0)
                        {
                            bg_row.FieldI = 1;
                        }
                        break;
                    case 10:
                        if (bg_row.FieldJ == 2)
                        {
                            bg_row.FieldJ = 3;
                        }
                        else if (bg_row.FieldJ == 0)
                        {
                            bg_row.FieldJ = 1;
                        }
                        break;
                }



                await _context.SaveChangesAsync();



            }

            OnGet();
           return Page();
        }

        private Boolean gameOver(Battleship.Models.Match match_row)
        {
            int count_hits_participant = 0;
            int count_hits_host = 0;

            for (int i = 1; i <= 10; i++)
            {
                //Hole Battleground row
                Battleground bg_row;

                bg_row = _context.Battlegrounds
                .Where(b => b.IdMatch == match_row.Id && b.FieldHost == false && b.YCord == i)
                .FirstOrDefault();

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

            if (count_hits_participant >= 30 || count_hits_host >= 30)
            {
                match_row.Done = true;
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

            
        }
    }
}
