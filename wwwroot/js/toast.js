const toasts = [];

export function addToast({ message, type = "info", duration = 115000 }) {
    const container = document.getElementById("toasts-container");

    if (!container) return;

    const toast = document.createElement("div");
    toast.className = `toast ${type}`;
    toast.textContent = message;

    toast.style.padding = "10px 15px";
    toast.style.marginBottom = "8px";
    toast.style.borderRadius = "4px";
    toast.style.color = "#fff";
    toast.style.fontFamily = "Arial, sans-serif";
    toast.style.boxShadow = "2px 2px 6px rgba(0,0,0,0.2)";
    toast.style.opacity = "0";
    toast.style.transition = "opacity 0.3s ease, transform 0.3s ease";
    toast.style.transform = "translateY(-20px)";
    toast.style.display = "block";

    switch (type) {
        case "success":
            toast.style.backgroundColor = "#4caf50";
            break;
        case "error":
            toast.style.backgroundColor = "#f44336";
            break;
        case "warning":
            toast.style.backgroundColor = "#ff9800";
            break;
        default:
            toast.style.backgroundColor = "#333";
    }

    container.appendChild(toast);
    toasts.push(toast);

    requestAnimationFrame(() => {
        toast.style.opacity = "1";
        toast.style.transform = "translateY(0)";
    });

    setTimeout(() => {
        toast.style.opacity = "0";
        toast.style.transform = "translateY(-20px)";
        setTimeout(() => {
            if (container.contains(toast)) container.removeChild(toast);
            const index = toasts.indexOf(toast);
            if (index > -1) toasts.splice(index, 1);
        }, 300);
    }, duration);
}

window.addToast = addToast;
