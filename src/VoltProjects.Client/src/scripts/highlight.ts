/**
 * Loads and highlights code with highlight.js
 */
export async function highlight(): Promise<void> {
	//Dynamically load highlight.js
	const hljs = await import('highlight.js');

	document.querySelectorAll('pre code').forEach(block => {
		hljs.default.highlightElement(block as HTMLElement);
	});
}