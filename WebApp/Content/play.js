function draw($square, data) {
    $square.val("");
    if (data != null) {
        $square.html('<img src="content/pieces/' + data.player + '_' + data.piece + '_alive.png" width="100%" height="100%"></img>');
    }
}

function update_chessboard() {
    var board = $.get("/api/game_info", {"gameId": gameId}, function(data) {
        $("#game_state").html(data.gameState);
        $("#active_player").html(data.activePlayer);
        for (var pos in data.squares)
            draw($("#sq-" + pos), data.squares[pos]);
    });
}

$(function() {
    update_chessboard();
});