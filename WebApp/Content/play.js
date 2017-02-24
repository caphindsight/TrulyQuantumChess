function draw($square, data) {
    $square.val("");
    if (data != null) {
        $square.val(data.player[0] + data.piece[0] + " " + data.probability);
    }
}

function initialize_chessboard(board) {
    $("#game_state").val(board.gameState);
    $("#active_player").val(board.activePlayer);
    for (var pos in board.squares)
        draw($("#sq-" + pos), board.squares[pos]);
}