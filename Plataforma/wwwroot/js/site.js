const menuToggles = document.querySelectorAll('.menu-toggle');

menuToggles.forEach(toggle => {
    toggle.addEventListener('click', function () {
        const targetId = this.dataset.target; // Get the ID of the nav to toggle
        const nav = document.querySelector(targetId);

        if (nav) {
            nav.classList.toggle('is-open');

            // Optional: Animate hamburger icon (e.g., turn into an X)
            // this.classList.toggle('is-active');
        }
    });
});