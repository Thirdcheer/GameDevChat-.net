$(".sidebar-dropdown > a").click(function () {

    if ($(this).parent().hasClass("active")) {
        $(this).next(".sidebar-submenu").slideUp(200);
        // $(".sidebar-dropdown").removeClass("active");
        $(this).parent().removeClass("active");
    }
    else {
        $(this).parent().addClass("active");
        $(this).next(".sidebar-submenu").slideDown(200);
        //$(".sidebar-dropdown").addClass("active");
    }

    $("#close-sidebar").click(function () {
        $(".page-wrapper").removeClass("toggled");
    });
    $("#show-sidebar").click(function () {
        $(".page-wrapper").addClass("toggled");
    });

});