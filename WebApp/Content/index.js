var g_captcha_response = "";

function captcha_callback(captcha_response) {
    g_captcha_response = captcha_response;
    $("#launch_new_game_btn").prop("disabled", false);
}

$(function() {
    $("#launch_new_game_btn").click(function() {
        $.get(prefix + "/api/new_game",
        {
            "captcha_response": g_captcha_response
        },
        function(data) {
            var gameId = data.gameId;
            window.location = prefix + "/play?gameId=" + gameId;
        });
    });
});