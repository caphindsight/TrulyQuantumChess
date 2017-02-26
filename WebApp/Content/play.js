var move = {
    move_type: "",
    source: "",
    middle: "",
    target: ""
};

function reset_move() {
    move.move_type = "";
    move.source = "";
    move.middle = "";
    move.target = "";
    $(".square-selected").removeClass("square-selected");
    $(".square-selected-quantum").removeClass("square-selected-quantum");
}

var prev_chessboard = null;

function squares_equal(a, b) {
    if (a == null && b == null) {
        return true;
    } else if (a != null && b == null) {
        return false;
    } else if (a == null && b != null) {
        return false;
    } else {
        if (a.player != b.player)
            return false;
        if (a.piece != b.piece)
            return false;
        if (a.probability != b.probability)
            return false;
        return true;
    }
}

function chessboards_equal(a, b) {
    if (a == null && b == null) {
        return true;
    } else if (a == null && b != null) {
        return false;
    } else if (a != null && b == null) {
        return false;
    } else {
        if (a.gameState != b.gameState)
            return false;
        if (a.activePlayer != b.activePlayer)
            return false;
        var rows = "abcdefgh";
        for (var x_i in rows) {
            var x = rows[x_i];
            for (var y = 1; y <= 8; y++) {
                var pos = x + y;
                if (!squares_equal(a.squares[pos], b.squares[pos]))
                    return false;
            }
        }
        return true;
    }
}

function submit_move() {
    if (move.middle == move.source || move.middle == move.target)
        move.middle = "";
    $.post(prefix + "/api/submit_move", {
         "gameId": gameId,
         "moveType": move.move_type,
         "source": move.source,
         "middle": move.middle,
         "target": move.target
     }, function(data) {
         if (!data.success) {
             $("#error_message").text(data.message);
         } else {
            $("#error_message").text("");
         }
         update_chessboard();
     }).fail(function(err) {
        alert("Move failed: " + err);
     }).always(function() {
        reset_move();
     });
}

function real_width(width, probability, piece_ratio) {
    var x = width * (1.0 - piece_ratio) / 2;
    return x + width * piece_ratio * probability;
}

function draw(canvas, data) {
    var ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    if (data != null) {
        var r = pieces_width_ratios[data.piece];
        var img_alive = new Image();
        img_alive.src = prefix + "/content/pieces/" + pieces_collection + "/" + data.player + "_" + data.piece + "_alive.png";
        img_alive.onload = function() {
            ctx.drawImage(img_alive, 0, 0, real_width(img_alive.width, data.probability, r), img_alive.height,
                0, 0, real_width(canvas.width, data.probability, r), canvas.height);
        };

        var img_dead = new Image();
        img_dead.src = prefix + "/content/pieces/" + pieces_collection + "/" + data.player + "_" + data.piece + "_dead.png";
        img_dead.onload = function() {
            ctx.drawImage(img_dead, real_width(img_dead.width, data.probability, r), 0, real_width(img_dead.width, 1.0 - data.probability, r), img_dead.height,
                real_width(canvas.width, data.probability, r), 0, real_width(canvas.width, 1.0 - data.probability, r), canvas.height);
        };
    }
}

function update_chessboard() {
    var board = $.get(prefix + "/api/game_info", {"gameId": gameId}, function(data) {
        if (chessboards_equal(prev_chessboard, data))
            return;
        if (data.gameState != "game_still_going") {
            $(".chessboard").addClass("game-over");
            var message = "???";
            if (data.gameState == "white_victory") {
                message = "White victory!";
            } else if (data.gameState == "black_victory") {
                message = "Black victory!";
            } else if (data.gameState == "tie") {
                message = "Players are tied!";
            } else {
                message = "Unknown game state: " + data.gameState;
            }
            $("#game_state").html("<h2>" + message + "</h2>");
            $("#new_game").show();
        }
        $("#active_player").text(data.activePlayer);
        for (var pos in data.squares) {
            if (prev_chessboard == null || !squares_equal(prev_chessboard.squares[pos], data.squares[pos]))
                draw($("#sq-" + pos)[0], data.squares[pos]);
        }
        prev_chessboard = data;
    });
}

$(function() {
    update_chessboard();
    setInterval(update_chessboard, 5000);
    var rows = "abcdefgh";

    // Setting up triggers
    for (var x_i in rows) {
        var x = rows[x_i];
        for (var y = 1; y <= 8; y++) {
            $("#sq-" + x + y).click(function(e) {
                var $elem = $(e.target);
                var pos = e.target.id[3] + e.target.id[4];

                if (move.source == "") {
                    // Selecting source square
                    move.move_type = "ordinary";
                    move.source = pos;
                    $elem.addClass("square-selected");
                } else if (move.source == pos && move.move_type == "ordinary") {
                    // Making move quantum
                    move.move_type = "quantum";
                    $elem.removeClass("square-selected");
                    $elem.addClass("square-selected-quantum");
                } else if (move.source == pos && move.move_type == "quantum") {
                    // Resetting move
                    reset_move();
                } else if (move.move_type == "quantum" && move.middle == "") {
                    move.middle = pos;
                    $elem.addClass("square-selected");
                } else {
                    move.target = pos;
                    submit_move();
                }
            });
        }
    }

    $("#capitulate_btn").click(function() {
        move.move_type = "capitulate";
        submit_move();
    });

    $("#castle_left_btn").click(function() {
        move.move_type = "castle_left";
        submit_move();
    });

    $("#castle_right_btn").click(function() {
        move.move_type = "castle_right";
        submit_move();
    });
});