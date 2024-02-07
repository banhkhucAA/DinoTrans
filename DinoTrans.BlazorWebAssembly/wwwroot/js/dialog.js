window.myJavaScriptFunction = function () {
    document.getElementById('my-dialog').showModal();
}


window.NotOpenDialog = function () {
    const elem = document.getElementById('my-dialog');
    elem.open = false;
}

window.ViewPictures = function (Id) {
    document.getElementById(`dialog-ViewPictures-${Id}`).showModal();
}

window.closeModalViewPictures = function (Id) {
    document.getElementById(`dialog-ViewPictures-${Id}`).close();
}

window.closeModal = function () {
    document.getElementById('my-dialog').close();
}