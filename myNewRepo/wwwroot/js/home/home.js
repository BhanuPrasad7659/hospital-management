/* CogMedi Hospital Management System - Home Page Script */

document.addEventListener('DOMContentLoaded', function () {
    console.log('wwwroot/js/home/home.js initialized');

    const wrapper = document.querySelector('.nav-dropdown-wrapper');
    const button = document.querySelector('.nav-login-btn');
    const dropdownMenu = document.querySelector('.nav-dropdown-menu');

    if (wrapper && button) {
        // Toggle dropdown open on button click
        button.addEventListener('click', function (e) {
            e.stopPropagation();
            wrapper.classList.toggle('open');
        });

        // Prevent closing when clicking inside the dropdown menu (unless clicking a portal link)
        if (dropdownMenu) {
            dropdownMenu.addEventListener('click', function (e) {
                if (e.target.closest('a')) {
                    wrapper.classList.remove('open');
                } else {
                    e.stopPropagation();
                }
            });
        }

        // Close dropdown when clicking anywhere outside
        document.addEventListener('click', function (e) {
            if (!wrapper.contains(e.target)) {
                wrapper.classList.remove('open');
            }
        });

        // Close dropdown on Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                wrapper.classList.remove('open');
            }
        });
    }
});
