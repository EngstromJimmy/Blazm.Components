import { BlazorSizeMedia } from './blazorSizeMedia.js';
let instance = new BlazorSizeMedia();
// Uncomment to enable console inspection of the instance
// (window as any).bs_instance = instance;
// When MQL is created, it tracks a NET Ref 
export function addMediaQueryList(dotnet) {
    instance.addMediaQueryList(dotnet);
}
// OnDispose this method cleans the MQL instance
export function removeMediaQueryList(dotnetMql) {
    instance.removeMediaQueryList(dotnetMql);
}
// When MediaQuery instance is created track media query in JavaScript
export function addMediaQueryToList(dotnetMql, mediaQuery) {
    return instance.addMediaQueryToList(dotnetMql, mediaQuery);
}
// When MediaQuery instance is disposed remove media query in JavaScript
export function removeMediaQuery(dotnetMql, mediaQuery) {
    instance.removeMediaQuery(dotnetMql, mediaQuery);
}
// Get media query value for cache after app loads
export function getMediaQueryArgs(mediaQuery) {
    return instance.getMediaQueryArgs(mediaQuery);
}
