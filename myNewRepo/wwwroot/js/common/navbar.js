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

/* ==========================================
   GLOBAL SEARCH BAR FUNCTIONALITY
   ========================================== */
document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('globalSearch');
    const resultsDropdown = document.getElementById('searchResultsDropdown');

    if (!searchInput || !resultsDropdown) return;

    let debounceTimer;

    searchInput.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        const query = searchInput.value.trim();

        if (query.length < 2) {
            resultsDropdown.innerHTML = '';
            resultsDropdown.classList.remove('active');
            return;
        }

        debounceTimer = setTimeout(() => {
            fetch(`/api/search?query=${encodeURIComponent(query)}`)
                .then(res => res.json())
                .then(data => {
                    resultsDropdown.innerHTML = '';
                    if (data.length === 0) {
                        resultsDropdown.innerHTML = '<div class="search-result-empty">No matching records found</div>';
                    } else {
                        data.forEach(item => {
                            const link = document.createElement('a');
                            link.href = item.url;
                            link.className = 'search-result-item';
                            
                            const title = document.createElement('span');
                            title.className = 'search-result-title';
                            title.textContent = item.title;

                            const subtitle = document.createElement('span');
                            subtitle.className = 'search-result-subtitle';
                            subtitle.textContent = item.subtitle;

                            link.appendChild(title);
                            link.appendChild(subtitle);
                            resultsDropdown.appendChild(link);
                        });
                    }
                    resultsDropdown.classList.add('active');
                })
                .catch(err => {
                    console.error('Error fetching search results:', err);
                });
        }, 300);
    });

    // Close dropdown on clicking outside
    document.addEventListener('click', (event) => {
        if (!searchInput.contains(event.target) && !resultsDropdown.contains(event.target)) {
            resultsDropdown.classList.remove('active');
        }
    });

    // Show dropdown again if focused and query is not empty
    searchInput.addEventListener('focus', () => {
        if (searchInput.value.trim().length >= 2) {
            resultsDropdown.classList.add('active');
        }
    });
});

/* ==========================================
   GLOBAL NOTIFICATION INTERACTION SYSTEM
   ========================================== */
function markReadAndRedirect(element, url) {
    if (element.classList.contains('unread')) {
        element.classList.remove('unread');
        
        // Decrement badge count
        const badge = document.getElementById('notifBadge');
        if (badge) {
            let count = parseInt(badge.textContent || '0', 10);
            if (count > 0) {
                count--;
                badge.textContent = count;
                if (count === 0) {
                    badge.style.display = 'none';
                }
            }
        }
    }
    
    // Redirect to the respective portal feature
    if (url) {
        window.location.href = url;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const markAllReadBtn = document.getElementById('markAllRead');
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', (e) => {
            e.stopPropagation(); // prevent dropdown closing
            
            // Mark all items as read
            const unreadItems = document.querySelectorAll('.notification-list li.unread');
            unreadItems.forEach(item => {
                item.classList.remove('unread');
            });
            
            // Dismiss badge
            const badge = document.getElementById('notifBadge');
            if (badge) {
                badge.textContent = '0';
                badge.style.display = 'none';
            }
        });
    }
});
