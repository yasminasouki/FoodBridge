const liveNotices = document.getElementById("live-notices");
const noticeBell = document.getElementById("notice-bell");
const noticeMenu = document.getElementById("notice-menu");
const noticeCount = document.getElementById("notice-count");
let notices = [];

if (noticeBell) {
    fetch(noticeBell.dataset.listUrl, { headers: { "X-Requested-With": "XMLHttpRequest" } })
        .then(response => response.ok ? response.json() : [])
        .then(items => {
            const ids = new Set(items.map(item => item.id));
            notices = notices.filter(item => !ids.has(item.id)).concat(items);
            renderNotices();
        })
        .catch(() => { });
}

if (liveNotices && noticeBell && window.signalR) {
    fetch(liveNotices.dataset.tokenUrl, { headers: { "X-Requested-With": "XMLHttpRequest" } })
        .then(response => response.ok ? response.text() : null)
        .then(token => {
            if (!token) {
                return;
            }

            const connection = new signalR.HubConnectionBuilder()
                .withUrl(liveNotices.dataset.hub, { accessTokenFactory: () => token })
                .withAutomaticReconnect()
                .build();
            connection.on("notice", showNotice);
            connection.start().catch(() => { });
        })
        .catch(() => { });
}

function showNotice(notice) {
    if (!notice || !notice.message) {
        return;
    }

    addNotice({
        id: notice.id,
        message: notice.message,
        createdAt: "Just now",
        isRead: false,
        href: noticeBell.dataset.openUrl + notice.id
    });

    const toast = document.createElement("div");
    toast.className = "toast align-items-center text-bg-success border-0";
    toast.setAttribute("role", "status");
    const body = document.createElement("div");
    body.className = "toast-body";
    body.textContent = notice.message;
    toast.appendChild(body);
    liveNotices.appendChild(toast);
    bootstrap.Toast.getOrCreateInstance(toast, { delay: 8000 }).show();
    toast.addEventListener("hidden.bs.toast", () => toast.remove());
}

function addNotice(item) {
    notices = [item, ...notices.filter(existing => existing.id !== item.id)];
    renderNotices();
}

function renderNotices() {
    if (!noticeMenu) {
        return;
    }

    noticeMenu.replaceChildren();
    if (notices.length === 0) {
        const empty = document.createElement("p");
        empty.className = "text-muted small px-3 py-2 mb-0";
        empty.textContent = "No notifications yet.";
        noticeMenu.append(empty);
        setCount(0);
        return;
    }

    let unread = 0;
    for (const item of notices) {
        if (!item.isRead) {
            unread += 1;
        }

        const link = document.createElement("a");
        link.className = "dropdown-item notice-item" + (item.isRead ? "" : " unread");
        link.href = item.href;
        const text = document.createElement("span");
        text.className = "d-block";
        text.textContent = item.message;
        const time = document.createElement("span");
        time.className = "d-block text-muted small";
        time.textContent = item.createdAt;
        link.append(text, time);
        noticeMenu.append(link);
    }

    setCount(unread);
}

function setCount(unread) {
    if (!noticeCount) {
        return;
    }

    if (unread <= 0) {
        noticeCount.classList.add("d-none");
        return;
    }

    noticeCount.textContent = String(unread);
    noticeCount.classList.remove("d-none");
}
