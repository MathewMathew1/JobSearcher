function addToast({ message, type = "info", duration = 22000 }) {
    let container = document.getElementById("toasts-container");
    if (!container) {
        container = document.createElement("div");
        container.id = "toasts-container";
        document.body.appendChild(container);
        Object.assign(container.style, {
            position: "fixed",
            top: "20px",
            right: "20px",
            display: "flex",
            flexDirection: "column",
            gap: "12px",
            zIndex: "9999",
            pointerEvents: "none"
        });
    }

    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <div style="display:flex;align-items:center;justify-content:space-between;gap:10px; flex:1; ">
            <div style="display:flex;align-items:center;gap:10px; flex:1;">
                ${getIcon(type)}
                <span style="flex:1;">${message}</span>
            </div>
            <button class="toast-close" style="background:none;border:none;color:white;font-size:18px;cursor:pointer;flex-shrink:0;">&times;</button>
        </div>
    `;

    Object.assign(toast.style, {
        padding: "12px 18px",
        borderRadius: "12px",
        color: "#fff",
        fontFamily: "system-ui, sans-serif",
        fontSize: "15px",
        backdropFilter: "blur(10px)",
        boxShadow: "0 4px 20px rgba(0, 0, 0, 0.25)",
        opacity: "0",
        transform: "translateY(-10px)",
        transition: "opacity 0.35s ease, transform 0.35s ease",
        pointerEvents: "auto",
        display: "flex",
        alignItems: "center"
    });

    switch (type) {
        case "success":
            toast.style.background = "linear-gradient(135deg, #28a745, #218838)";
            break;
        case "error":
            toast.style.background = "linear-gradient(135deg, #dc3545, #b02a37)";
            break;
        case "warning":
            toast.style.background = "linear-gradient(135deg, #ffc107, #d39e00)";
            toast.style.color = "#222";
            break;
        default:
            toast.style.background = "linear-gradient(135deg, #0d6efd, #0b5ed7)";
    }

    container.appendChild(toast);

    // Animate in
    requestAnimationFrame(() => {
        toast.style.opacity = "1";
        toast.style.transform = "translateY(0)";
    });

    // Close button functionality
    toast.querySelector(".toast-close").addEventListener("click", () => {
        removeToast(toast);
    });

    // Auto-remove after duration
    const timeout = setTimeout(() => {
        removeToast(toast);
    }, duration);

    function removeToast(t) {
        clearTimeout(timeout);
        t.style.opacity = "0";
        t.style.transform = "translateY(-10px)";
        setTimeout(() => {
            if (container.contains(t)) container.removeChild(t);
        }, 350);
    }
}

function getIcon(type) {
    const size = 18;
    switch (type) {
        case "success":
            return `<svg width="${size}" height="${size}" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>`;
        case "error":
            return `<svg width="${size}" height="${size}" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>`;
        case "warning":
            return `<svg width="${size}" height="${size}" fill="none" stroke="black" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polygon points="12 2 22 20 2 20"></polygon><line x1="12" y1="8" x2="12" y2="13"></line><circle cx="12" cy="17" r="1"></circle></svg>`;
        default:
            return `<svg width="${size}" height="${size}" fill="none" stroke="white" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><circle cx="12" cy="8" r="1"></circle></svg>`;
    }
}

window.addToast = addToast;
