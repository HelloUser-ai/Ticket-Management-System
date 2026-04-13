function toggleSidebar() {
    const sidebar = document.getElementById("sidebar");
    const mainContent = document.getElementById("mainContent");
    const icon = document.getElementById("menuIcon");
    sidebar.classList.toggle("show");
    mainContent.classList.toggle("sidebar-open");
    icon.className = sidebar.classList.contains("show") ? "bi bi-x-lg" : "bi bi-list";
}

const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();
connection.start().catch(err => console.error(err));

let page = 1;

connection.on("ReceiveNotification", function (data) {
    toastr.success(data.message);
    page = 1;
    loadNotifications();
});

function loadNotifications() {
    fetch('/Notification/GetNotifications?page=' + page)
        .then(res => res.json())
        .then(res => {
            let list = document.querySelector(".notif-body");
            let loadMoreBtn = document.getElementById("loadMoreBtn");

            if (res.total === 0) {
                list.innerHTML = `
                <div class="p-4 text-center">
                    <i class="bi bi-bell-slash text-muted fs-2"></i>
                    <div class="text-muted small mt-2">No new notifications</div>
                </div>`;
                if (loadMoreBtn) loadMoreBtn.style.display = "none";
                document.getElementById("notifCount").innerText = "0";
                return;
            }

            if (page === 1) list.innerHTML = "";

            let currentItemsCount = list.querySelectorAll('.notif-item').length;

            let html = res.data.map((n, i) => {
                let rowNumber = currentItemsCount + (i + 1);
                return `
                <div class="notif-item">
                    <div class="d-flex align-items-start">
                        <span class="notif-number">${rowNumber}</span>
                        <div class="flex-grow-1">
                            <div class="fw-bold small text-dark">${n.title}</div>
                            <div class="text-muted" style="font-size: 0.7rem;">${n.message}</div>
                        </div>
                    </div>
                </div>`;
            }).join('');

            list.innerHTML += html;

            let totalRendered = list.querySelectorAll('.notif-item').length;
            if (totalRendered < res.total) {
                loadMoreBtn.style.display = "block";
            } else {
                loadMoreBtn.style.display = "none";
            }

            document.getElementById("notifCount").innerText = res.total;
        });
}

function loadMore(e) {
    if (e) e.stopPropagation();
    page++;
    loadNotifications();
}

loadNotifications();
