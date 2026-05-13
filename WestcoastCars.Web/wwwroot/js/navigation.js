$(document).ready(function () {
    const sidebar = $('#sidebar');
    const overlay = $('#sidebarOverlay');
    const body = $('body');

    function openSidebar() {
        sidebar.addClass('active').attr('aria-hidden', 'false');
        overlay.addClass('active');
        body.addClass('drawer-open');
    }

    function closeSidebar() {
        sidebar.removeClass('active').attr('aria-hidden', 'true');
        overlay.removeClass('active');
        body.removeClass('drawer-open');
    }

    $('#menuBtn').on('click', openSidebar);
    $('#closeBtn, #sidebarOverlay').on('click', closeSidebar);
    $('#sidebar a').on('click', closeSidebar);

    $(document).on('keyup', function (event) {
        if (event.key === 'Escape') {
            closeSidebar();
        }
    });
});
