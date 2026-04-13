function togglePass() {
    var x = document.getElementById("myPassword");
    var icon = document.getElementById("eyeIcon");
    if (x.type === "password") {
        x.type = "text";
        icon.classList.replace("bi-eye-fill", "bi-eye-slash-fill");
        x.style.letterSpacing = "0px";
    } else {
        x.type = "password";
        icon.classList.replace("bi-eye-slash-fill", "bi-eye-fill");
        x.style.letterSpacing = "2px";
    }
}
