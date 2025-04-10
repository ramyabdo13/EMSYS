$(document).ready(function () {






    $("#Show_Hide").removeClass("fa-eye").addClass("fa-eye-slash");

    $("#Show_Hide").click(function () {
        const passwordInput = $("#Password");
        const type = passwordInput.attr("type");

        if (type === "password") {
            passwordInput.attr("type", "text");
            $("#Show_Hide").removeClass("fa-eye-slash").addClass("fa-eye");
        } else {
            passwordInput.attr("type", "password");
            $("#Show_Hide").removeClass("fa-eye").addClass("fa-eye-slash");



        }
    });
});
