@page
@model Battleship.Pages.GameModel
@{
    <h1>Willkommen im Spiel</h1>

    @if (TempData["Message"] != null)
    {
        <div class="alert alert-success" role="alert">
            @TempData["Message"]
        </div>
    }
    else
    {
        <h2>Eigene Map</h2>
        <table border="1">
            <tr>
                <th>0</th>
                <th>1</th>
                <th>2</th>
                <th>3</th>
                <th>4</th>
                <th>5</th>
                <th>6</th>
                <th>7</th>
                <th>8</th>
                <th>9</th>
                <th>10</th>
            </tr>
            @for (int i = 0; i < 10; i++)
            {
                <tr>
                    <th>@(i + 1)</th>
                    @for (int j = 0; j < 10; j++)
                    {
                        <td>
                            @Model.OwnMap[i][j]
                        </td>
                    }
                </tr>
            }
        </table>
        <br>
        <br>
        <br>

        <h2>Gegnerische Map</h2>
        <table border="1">
            <tr>
                <th>0</th>
                <th>1</th>
                <th>2</th>
                <th>3</th>
                <th>4</th>
                <th>5</th>
                <th>6</th>
                <th>7</th>
                <th>8</th>
                <th>9</th>
                <th>10</th>
            </tr>
            @for (int i = 0; i < 10; i++)
            {
                <tr>
                    <th>@(i + 1)</th>
                    @for (int j = 0; j < 10; j++)
                    {
                        <td>
                            @if (Model.myTurn)
                            {
                                <form method="post">
                                    <input type="hidden" asp-for="y_cord" value="@(i + 1)" />
                                    <input type="hidden" asp-for="x_cord" value="@(j + 1)" />
                                    <input type="submit" value="@Model.OpponentMap[i][j]" />
                                </form>
                            }
                            else
                            {
                                @Model.OpponentMap[i][j]
                            }


                        </td>
                    }
                </tr>
            }
        </table>

    }
    
}
