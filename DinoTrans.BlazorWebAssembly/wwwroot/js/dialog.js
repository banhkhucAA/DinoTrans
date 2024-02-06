window.myJavaScriptFunction = function () {
    document.getElementById('my-dialog').showModal();
}


window.NotOpenDialog = function () {
    const elem = document.getElementById('my-dialog');
    elem.open = false;
}

window.ViewPictures = function () {
    document.getElementById('dialog-ViewPictures').showModal();;
}

window.closeModalViewPictures = function () {
    document.getElementById('dialog-ViewPictures').close();
}


window.closeModal = function () {
    document.getElementById('my-dialog').close();
}