const cursoItems = document.querySelectorAll('.curso-item');

cursoItems.forEach(item => {
    const header = item.querySelector('.curso-header');
    const modulesList = item.querySelector('.modules-list');
    const toggleButton = item.querySelector('.toggle-modules');
    const chevronIcon = toggleButton.querySelector('i');

    header.addEventListener('click', function () {
        modulesList.classList.toggle('open');
        chevronIcon.classList.toggle('rotated');
    });

    toggleButton.addEventListener('click', function (event) {
        event.stopPropagation();
        modulesList.classList.toggle('open');
        chevronIcon.classList.toggle('rotated');
    });
});