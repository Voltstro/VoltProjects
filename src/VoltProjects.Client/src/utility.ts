export function isVisible(element: Element): boolean {
    return (element as HTMLElement).offsetParent != null;
}

export function debounce(func: () => void, timeout = 300): () => void {
    let timer: string | number | NodeJS.Timeout;
    return (...args: any) => {
        clearTimeout(timer);
        timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
}
