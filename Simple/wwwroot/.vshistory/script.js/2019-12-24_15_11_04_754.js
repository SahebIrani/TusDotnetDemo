

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
    uploadProgress.value = 0;
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
    uploadProgress.style.width = percentage + 'px';
    setProgressTest(bytesUploaded + '/' + bytesTotal + ' bytes uploaded');
}

function onTusSuccess() {
    downloadLink.innerHTML = '<a class="btn-link text-info" href="' + upload.url + '">Download ' + upload.file.name + '</a>';
    enableUpload();
}

function setProgressTest(text) {
    uploadProgress.setAttribute('data-label', text);
}

function enableUpload() {
    uploadButton.removeAttribute('disabled');
    cancelUploadButton.setAttribute('disabled', 'disabled');
}

function disableUpload() {
    uploadButton.setAttribute('disabled', 'disabled');
    cancelUploadButton.removeAttribute('disabled');
}