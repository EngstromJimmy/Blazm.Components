export function OpenInNewWindow(url, message) {
    var newwindow = window.open('', '_blank');
    newwindow.document.write(message);
    newwindow.location.href = url;
}