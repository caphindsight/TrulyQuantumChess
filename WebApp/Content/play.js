function draw(canvas, data) {
    var ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    if (data != null) {
        var img_alive = new Image();
        img_alive.src = "/content/pieces/" + data.player + "_" + data.piece + "_alive.png";
        img_alive.onload = function() {
            ctx.drawImage(img_alive, 0, 0, img_alive.width * data.probability, img_alive.height,
                0, 0, canvas.width * data.probability, canvas.height);
        };

        var img_dead = new Image();
        img_dead.src = "/content/pieces/" + data.player + "_" + data.piece + "_dead.png";
        img_dead.onload = function() {
            ctx.drawImage(img_dead, img_dead.width * data.probability, 0, img_dead.width * (1.0 - data.probability), img_dead.height,
                canvas.width * data.probability, 0, canvas.width * (1.0 - data.probability), canvas.height);
        };
    }
}

function update_chessboard() {
    var board = $.get("/api/game_info", {"gameId": gameId}, function(data) {
        $("#game_state").html(data.gameState);
        $("#active_player").html(data.activePlayer);
        for (var pos in data.squares)
            draw($("#sq-" + pos)[0], data.squares[pos]);
    });
}

$(function() {
    update_chessboard();
});