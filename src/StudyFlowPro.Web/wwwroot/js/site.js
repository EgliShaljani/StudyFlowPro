(function () {
    const root = document.documentElement;
    const body = document.body;
    const themeToggle = document.getElementById("themeToggle");
    const themeLabel = themeToggle?.querySelector("[data-theme-label]");
    const storageKey = "studyflow-theme";
    const notificationToggle = document.getElementById("notificationToggle");
    const notificationPanel = document.getElementById("notificationPanel");
    const notificationList = document.getElementById("notificationList");
    const notificationEmpty = document.getElementById("notificationEmpty");
    const notificationCount = document.getElementById("notificationCount");
    const toastStack = document.getElementById("notificationToastStack");

    let lastDigestFingerprint = "";

    function applyTheme(theme) {
        const normalizedTheme = theme === "dark" ? "dark" : "light";
        root.dataset.theme = normalizedTheme;

        if (themeLabel) {
            themeLabel.textContent = normalizedTheme === "dark" ? "Light mode" : "Dark mode";
        }

        if (themeToggle) {
            themeToggle.setAttribute("aria-pressed", String(normalizedTheme === "dark"));
        }
    }

    const preferredTheme = localStorage.getItem(storageKey) ??
        (window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");
    applyTheme(preferredTheme);

    themeToggle?.addEventListener("click", function () {
        const nextTheme = root.dataset.theme === "dark" ? "light" : "dark";
        localStorage.setItem(storageKey, nextTheme);
        applyTheme(nextTheme);
    });

    notificationToggle?.addEventListener("click", function () {
        if (!notificationPanel) {
            return;
        }

        const nextHidden = !notificationPanel.hidden;
        notificationPanel.hidden = nextHidden;
        notificationToggle.setAttribute("aria-expanded", String(!nextHidden));
    });

    document.addEventListener("click", function (event) {
        if (!notificationPanel || !notificationToggle || notificationPanel.hidden) {
            return;
        }

        const target = event.target;
        if (!(target instanceof Node)) {
            return;
        }

        if (!notificationPanel.contains(target) && !notificationToggle.contains(target)) {
            notificationPanel.hidden = true;
            notificationToggle.setAttribute("aria-expanded", "false");
        }
    });

    function createPill(className, text) {
        const span = document.createElement("span");
        span.className = className;
        span.textContent = text;
        return span;
    }

    function renderNotificationItem(item) {
        const article = document.createElement("article");
        article.className = "notification-item";

        const topRow = document.createElement("div");
        topRow.className = "notification-item-top";

        const textBlock = document.createElement("div");
        const title = document.createElement("strong");
        title.textContent = item.title;
        const details = document.createElement("small");
        details.textContent = `${item.subjectName} • ${item.message}`;
        textBlock.append(title, details);

        const date = document.createElement("span");
        date.className = "notification-item-date";
        date.textContent = item.dueDate;

        topRow.append(textBlock, date);

        const tags = document.createElement("div");
        tags.className = "notification-item-tags";
        tags.append(
            createPill(`priority-pill priority-${item.priority.toLowerCase()}`, item.priority),
            createPill(`status-pill status-${item.status.replace(" ", "").toLowerCase()}`, item.status)
        );

        article.append(topRow, tags);
        return article;
    }

    function showToast(item) {
        if (!toastStack) {
            return;
        }

        const toast = document.createElement("article");
        toast.className = "app-toast";

        const title = document.createElement("strong");
        title.textContent = item.title;

        const copy = document.createElement("span");
        copy.textContent = `${item.subjectName} • ${item.message}`;

        toast.append(title, copy);
        toastStack.appendChild(toast);

        window.setTimeout(function () {
            toast.remove();
        }, 4200);
    }

    function updateNotificationDigest(digest) {
        if (!notificationList || !notificationEmpty || !notificationCount) {
            return;
        }

        const items = Array.isArray(digest?.items) ? digest.items : [];
        const nextFingerprint = JSON.stringify(items.map(function (item) {
            return [item.id, item.isOverdue, item.dueDate, item.status];
        }));

        notificationList.replaceChildren();

        if (items.length === 0) {
            notificationEmpty.hidden = false;
        } else {
            notificationEmpty.hidden = true;
            items.forEach(function (item) {
                notificationList.appendChild(renderNotificationItem(item));
            });
        }

        notificationCount.textContent = String(items.length);

        if (lastDigestFingerprint && lastDigestFingerprint !== nextFingerprint) {
            items.slice(0, 2).forEach(showToast);
        }

        lastDigestFingerprint = nextFingerprint;
    }

    if (body?.dataset.authenticated !== "true" || typeof window.signalR === "undefined") {
        return;
    }

    const connection = new window.signalR.HubConnectionBuilder()
        .withUrl("/hubs/deadlines")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveDeadlineDigest", updateNotificationDigest);

    connection.start()
        .then(function () {
            return connection.invoke("RequestRefresh");
        })
        .catch(function () {
            /* Ignore transient notification startup failures. */
        });
})();
