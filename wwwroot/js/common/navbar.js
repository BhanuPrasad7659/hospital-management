/* ==========================================
   COGMEDI TOP NAVBAR DROPDOWN INTERACTION
   ========================================== */

function toggleDropdown(dropdownId) {
    const dropdown = document.getElementById(dropdownId);
    if (!dropdown) return;

    // Close all other active dropdowns first
    const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');
    activeDropdowns.forEach(d => {
        if (d.id !== dropdownId) {
            d.classList.remove('active');
        }
    });

    // Toggle target dropdown
    dropdown.classList.toggle('active');
}

// Global click event to close dropdowns when clicking outside
document.addEventListener('click', (event) => {
    const isClickInsideNotification = event.target.closest('#notificationWrapper');
    const isClickInsideProfile = event.target.closest('#profileWrapper');

    if (!isClickInsideNotification && !isClickInsideProfile) {
        const activeDropdowns = document.querySelectorAll('.dropdown-menu.active');
        activeDropdowns.forEach(d => {
            d.classList.remove('active');
        });
    }
});
