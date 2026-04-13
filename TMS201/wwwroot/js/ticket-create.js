let dt = new DataTransfer();

function handleFileSelect(input) {
    const list = document.getElementById('filePreviewList');
    const { files } = input;
    const MAX_SIZE_BYTES = 50 * 1024 * 1024;

    for (let i = 0; i < files.length; i++) {
        const file = files[i];

        if (file.size > MAX_SIZE_BYTES) {
            alert(`Bhai, "${file.name}" bahut badi hai (50MB limit)!`);
            continue;
        }

        dt.items.add(file);

        const icon = getFileIcon(file.name);
        const item = document.createElement('div');
        item.className = 'file-preview-item';
        item.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="${icon} fs-5 me-3 text-info"></i>
                <div>
                    <div class="small fw-bold text-white">${file.name}</div>
                    <div class="text-muted" style="font-size: 0.7rem;">${(file.size / 1024 / 1024).toFixed(2)} MB</div>
                </div>
            </div>
            <i class="bi bi-trash btn-remove" onclick="removeFile(this, '${file.name}')"></i>
        `;
        list.appendChild(item);
    }
    input.files = dt.files;
}

function removeFile(btn, fileName) {
    const input = document.getElementById('fileInput');
    btn.parentElement.remove();

    const newDt = new DataTransfer();
    for (let i = 0; i < dt.files.length; i++) {
        if (dt.files[i].name !== fileName) {
            newDt.items.add(dt.files[i]);
        }
    }
    dt = newDt;
    input.files = dt.files;
}

function getFileIcon(fileName) {
    const ext = fileName.split('.').pop().toLowerCase();
    if (['jpg', 'jpeg', 'png', 'gif'].includes(ext)) return 'bi-file-earmark-image';
    if (['mp4', 'mov', 'avi'].includes(ext)) return 'bi-file-earmark-play';
    if (['mp3', 'wav', 'ogg'].includes(ext)) return 'bi-file-earmark-music';
    if (['pdf'].includes(ext)) return 'bi-file-earmark-pdf';
    if (['xls', 'xlsx'].includes(ext)) return 'bi-file-earmark-excel';
    if (['doc', 'docx'].includes(ext)) return 'bi-file-earmark-word';
    if (['zip', 'rar', '7z'].includes(ext)) return 'bi-file-earmark-zip';
    return 'bi-file-earmark-text';
}
