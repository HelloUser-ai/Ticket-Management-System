document.getElementById('eyeIcon').addEventListener('click', function () {
    const passInput = document.getElementById('passwordField');

    if (passInput.type === 'password') {
        passInput.type = 'text';
        this.classList.remove('bi-eye-slash');
        this.classList.add('bi-eye');
    } else {
        passInput.type = 'password';
        this.classList.remove('bi-eye');
        this.classList.add('bi-eye-slash');
    }
});

const myModalEl = document.getElementById('retrieveModal');
myModalEl.addEventListener('hidden.bs.modal', function () {
    document.getElementById('retrieveUsername').value = '';
    const resultDiv = document.getElementById('passwordResult');
    resultDiv.classList.add('d-none');
    resultDiv.innerHTML = 'Your Password: <strong id="showPass"></strong>';
});

async function fetchPassword() {
    const username = document.getElementById('retrieveUsername').value;
    const resultDiv = document.getElementById('passwordResult');
    const showPass = document.getElementById('showPass');

    if (!username) {
        alert("Bhai, username toh daalo!");
        return;
    }

    try {
        const response = await fetch(`/Account/GetUserPassword?username=${username}`, {
            method: 'POST'
        });

        const data = await response.json();

        if (data.success) {
            resultDiv.className = "alert alert-success mt-3";
            resultDiv.innerHTML = `Success! Password for <b>${username}</b> is: <br><span class="fs-5 fw-bold">${data.password}</span>`;
            resultDiv.classList.remove('d-none');
        } else {
            resultDiv.className = "alert alert-danger mt-3";
            resultDiv.innerHTML = `<i class="bi bi-exclamation-triangle-fill"></i> ${data.message}`;
            resultDiv.classList.remove('d-none');
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Server se connection nahi ho pa raha!");
    }
}
