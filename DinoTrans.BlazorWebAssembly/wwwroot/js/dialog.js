window.myJavaScriptFunction = function () {
    document.getElementById('my-dialog').showModal();
}


window.NotOpenDialog = function () {
    const elem = document.getElementById('my-dialog');
    elem.open = false;
}


window.closeModal = function () {
    document.getElementById('my-dialog').close();
}