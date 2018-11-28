// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function refreshMatch() {
    $.get("Match/LaunchRefreshMatch");
    $("#refreshButton > span").addClass("gly-spin");
    $("#refreshButton").attr("disabled", "disabled");
    justEnd = true;
}
var justEnd = false;
function GetRefreshMatch() {
    $.get("Match/IsResfreshingMatch", function (data) {
        if (data === true) {
            $("#refreshButton > span").addClass("gly-spin");
            $("#refreshButton").attr("disabled", "disabled");
            justEnd = true;
        }
        else
        {
            $("#refreshButton > span").removeClass("gly-spin");
            $("#refreshButton").removeAttr("disabled", "disabled");
            if (justEnd) {
                window.location.reload();
                justEnd = false;
            }

        }
    });
    setTimeout(arguments.callee, 5000);
}

$("#refreshButton").click(refreshMatch);
$(document).ready(GetRefreshMatch);