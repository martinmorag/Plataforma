const toggleButtons = document.querySelectorAll('.toggle-modules-btn');

toggleButtons.forEach(button => {
    button.addEventListener('click', function () {
        const modulesList = this.closest('.course-item').querySelector('.modules-list');
        const icon = this.querySelector('i');

        // Toggle the 'open' class for CSS transition
        modulesList.classList.toggle('open');
        // Toggle aria-expanded for accessibility
        const isExpanded = this.getAttribute('aria-expanded') === 'true';
        this.setAttribute('aria-expanded', !isExpanded);
        // Toggle the icon rotation
        icon.classList.toggle('fa-chevron-down');
        icon.classList.toggle('fa-chevron-up'); // Or just add/remove a 'rotated' class as per your original CSS
    });
});