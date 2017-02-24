$(function() {
    $("#play_btn").click(function() {
        $.get("/api/new_game", {}, function(data) {
            var gameId = data.gameId;
            window.location = "/play?gameId=" + gameId;
        });
    });
});