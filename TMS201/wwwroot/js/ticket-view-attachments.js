function showPreview(url, type, fileName) {
    const container = document.getElementById('previewContent');
    const title = document.getElementById('modalFileName');
    if (!container) return;

    title.innerText = fileName;
    container.innerHTML = '<div class="text-white">Loading...</div>';

    if (type.includes('image')) {
        container.innerHTML = `<img src="${url}" class="img-fluid p-2" style="max-height: 80vh; object-fit: contain;" />`;
    } else if (type.includes('pdf')) {
        container.innerHTML = `<iframe src="${url}" width="100%" height="600px" style="border:none;"></iframe>`;
    } else if (type.includes('video')) {
        container.innerHTML = `<video width="100%" height="400px" controls class="bg-black"><source src="${url}" type="${type}"></video>`;
    } else if (type.includes('audio')) {
        container.innerHTML = `<div class="p-5"><audio controls style="width: 90%;"><source src="${url}" type="${type}"></audio></div>`;
    } else {
        container.innerHTML = `<div class="p-5 text-white"><h5>No preview available.</h5></div>`;
    }

    var myModal = new bootstrap.Modal(document.getElementById('previewModal'));
    myModal.show();
}
