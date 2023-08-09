export function isVisible(element: Element): boolean {
	return (element as HTMLElement).offsetParent != null;
}
