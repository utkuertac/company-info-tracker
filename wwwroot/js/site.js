// Sayfa yüklendiğinde çalışacak
document.addEventListener("DOMContentLoaded", function () {
    // Şifreleri göster/gizle: body üzerindeki sınıf tüm .secret alanlarının bulanıklığını kontrol eder
    const toggleButton = document.getElementById("toggle-password-blur");

    if (toggleButton) {
        toggleButton.addEventListener("click", function () {
            const visible = document.body.classList.toggle("secrets-visible");
            toggleButton.textContent = visible ? "🔒 Şifreleri Gizle" : "🔓 Şifreleri Göster";
        });
    }

    // Çift tıklanan hücrenin değerini panoya kopyala
    document.querySelectorAll(".copyable").forEach(cell => {
        cell.addEventListener("dblclick", () => copyToClipboard(cell.dataset.copy));
    });

    // Silme gibi geri alınamaz işlemler için onay iste
    document.querySelectorAll("form[data-confirm]").forEach(form => {
        form.addEventListener("submit", e => {
            if (!confirm(form.dataset.confirm)) {
                e.preventDefault();
            }
        });
    });
});

async function copyToClipboard(text) {
    if (!text) return;

    try {
        await navigator.clipboard.writeText(text);
    } catch {
        // Clipboard API kullanılamıyorsa (ör. HTTPS olmayan ortam) eski yönteme dön
        const el = document.createElement("textarea");
        el.value = text;
        document.body.appendChild(el);
        el.select();
        document.execCommand("copy");
        document.body.removeChild(el);
    }

    const toast = document.getElementById("copy-toast");
    if (toast) {
        toast.style.display = "block";
        setTimeout(() => {
            toast.style.display = "none";
        }, 1500);
    }
}
