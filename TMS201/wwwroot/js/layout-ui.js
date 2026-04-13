
document.addEventListener("click", function (e) {
    const sidebar = document.getElementById("sidebar");
    const mainContent = document.getElementById("mainContent");
    const icon = document.getElementById("menuIcon");

    if (!sidebar) return;

    const isSidebarOpen = sidebar.classList.contains("show");

    if (isSidebarOpen) {
        const clickedInsideSidebar = sidebar.contains(e.target);
        const clickedToggleBtn = e.target.closest('[onclick="toggleSidebar()"]');

        if (!clickedInsideSidebar && !clickedToggleBtn) {
            sidebar.classList.remove("show");
            mainContent.classList.remove("sidebar-open");

            if (icon) {
                icon.className = "bi bi-list";
            }
        }
    }
});

toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-bottom-right",
    "timeOut": "3000",
    "extendedTimeOut": "1000",
    "showMethod": "fadeIn",
    "hideMethod": "fadeOut"
};
