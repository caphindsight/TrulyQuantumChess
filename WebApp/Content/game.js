function initialize_chessboard(board) {
    $("#game_state").val(board.gameState);
    $("#active_player").val(board.activePlayer);
    for (var pos in board.squares) {
        var square = board.squares[pos];
        // TODO: actually draw the chessboard
    }
}