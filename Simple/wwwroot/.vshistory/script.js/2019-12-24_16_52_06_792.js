

var uploadProgressParentBT = document.getElementById('uploadProgressParentBT');
var uploadProgressBT = document.getElementById('uploadProgressBT');
var uploadProgress = document.getElementById('uploadProgress');
var downloadLink = document.getElementById('downloadLink');
var cancelUploadButton = document.getElementById('cancelUploadButton');
var uploadButton = document.getElementById('uploadButton');
var upload;

function uploadFile() {
    var file = document.getElementById('droppedFile').files[0];

    uploadProgress.value = 0;
    uploadProgress.removeAttribute('data');
    uploadProgress.style.display = 'block';

    uploadProgressParentBT.style.display = 'flex';
    uploadProgressBT.setAttribute('aria-valuenow', '0');
    uploadProgressBT.style.width = '0%';

    disableUpload();

    downloadLink.innerHTML = '';

    upload = new tus.Upload(file,
        {
            endpoint: 'files/',
            onError: onTusError,
            onProgress: onTusProgress,
            onSuccess: onTusSuccess,
            metadata: {
                name: file.name,
                contentType: file.type || 'application/octet-stream'
            }
        });

    setProgressTest('Starting upload...');
    upload.start();
}

function cancelUpload() {
    upload && upload.abort();
    setProgressTest('Upload aborted');
    //uploadProgress.value = 0;
    uploadProgressBT.classList.remove("bg-info");
    uploadProgressBT.classList.add("bg-danger");
    enableUpload();
}

function resetLocalCache(e) {
    e.preventDefault();
    localStorage.clear();
    alert('Cache cleared');
}

function onTusError(error) {
    alert(error);
    enableUpload();
}

function onTusProgress(bytesUploaded, bytesTotal) {
    var percentage = (bytesUploaded / bytesTotal * 100).toFixed(2);

    uploadProgress.value = percentage;

    uploadProgressBT.setAttribute('aria-valuenow', percentage);
    uploadProgressBT.style.width = percentage + '%';
    uploadProgressBT.classList.remove("bg-danger");
    uploadProgressBT.classList.add("bg-info");

    setProgressTest(bytesUploaded + '/' + bytesTotal + ' bytes uploaded');
}

function onTusSuccess() {
    downloadLink.innerHTML = '<a class="btn-link text-info" href="' + upload.url + '">Download ' + upload.file.name + '</a>';
    enableUpload();
}

function setProgressTest(text) {
    uploadProgress.setAttribute('data-label', text);
    uploadProgressBT.innerText = text;
}

function enableUpload() {
    uploadButton.removeAttribute('disabled');
    cancelUploadButton.setAttribute('disabled', 'disabled');
}

function disableUpload() {
    uploadButton.setAttribute('disabled', 'disabled');
    cancelUploadButton.removeAttribute('disabled');
}