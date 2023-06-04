import { ResizeListener } from './blazorSizeResize.js';
let instance = new ResizeListener();
export function listenForResize(dotnetRef, options) {
    instance.listenForResize(dotnetRef, options);
}
export function cancelListener() {
    instance.cancelListener();
}
export function matchMedia(query) {
    return instance.matchMedia(query);
}
export function getBrowserWindowSize() {
    return instance.getBrowserWindowSize();
}
