/* ==========================================
   COGMEDI SIDEBAR TRANSITION INTERACTION
   ========================================== */

function toggleSidebar() {
    const container = document.querySelector('.app-container');
    if (!container) return;

    container.classList.toggle('sidebar-collapsed');

    // Persist collapsed state to localStorage
    const isCollapsed = container.classList.contains('sidebar-collapsed');
    localStorage.setItem('cogmedi_sidebar_collapsed', isCollapsed);

    // Re-adjust Chart.js canvas sizes if charts exist on the page
    setTimeout(() => {
        const canvases = document.querySelectorAll('canvas');
        canvases.forEach(canvas => {
            const chartInstance = Chart.getChart(canvas);
            if (chartInstance) {
                chartInstance.resize();
            }
        });
    }, 300); // Wait for CSS transition
}

// Apply persisted state on document load
document.addEventListener("DOMContentLoaded", () => {
    const container = document.querySelector('.app-container');
    if (!container) return;

    const isCollapsed = localStorage.getItem('cogmedi_sidebar_collapsed') === 'true';
    if (isCollapsed) {
        container.classList.add('sidebar-collapsed');
    }
});
