const passwordInput = document.getElementById('passwordInput');
const togglePassword = document.getElementById('togglePassword');
const eyeIcon = document.getElementById('eyeIcon');
const eyeSlashIcon = document.getElementById('eyeSlashIcon');

togglePassword.addEventListener('click', function () {
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        eyeIcon.classList.add('d-none');
        eyeSlashIcon.classList.remove('d-none');
    } else {
        passwordInput.type = 'password';
        eyeIcon.classList.remove('d-none');
        eyeSlashIcon.classList.add('d-none');
    }
});