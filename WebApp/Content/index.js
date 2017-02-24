$(function() {
    $("#play_btn").click(function() {
        $.get(prefix + "/api/new_game", {}, function(data) {
            var gameId = data.gameId;
            window.location = prefix + "/play?gameId=" + gameId;
        });
    });
});