/**
 * Adds anchors to all 
 */
export async function enableAnchor(): Promise<void> {
	//Dynamically load anchor-js
	const AnchorJs = await import('anchor-js');
	const anchors = new AnchorJs.default();
	anchors.options = {
		placement: 'right',
		visible: 'hover'
	};
	anchors.add('article h2,h3');
}