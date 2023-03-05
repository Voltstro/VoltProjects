export function renderToc(): void {
	const tocElement = document.getElementById('toc');
	if (!tocElement) {
		return;
	}

	createClickFunctionsOnElement(tocElement);
}

function createClickFunctionsOnElement(element: HTMLElement): void {
	const ulNode = Array.from(element.childNodes).find((x) => x.nodeName === 'UL');
	if(!ulNode)
		return;

	Array.from(ulNode.childNodes).map((childNode) => {
		const childrenArray = Array.from(childNode.childNodes);
		const iElement = childrenArray.find((x) => x.nodeName === 'I');
		if(iElement) {
			iElement.addEventListener('click', () => {
				if(iElement.parentElement.hasAttribute('class')) {
					iElement.parentElement.removeAttribute('class');
				} else {
					iElement.parentElement.setAttribute('class', 'active');
				}
					
				console.log('Click');
			});
		}

		const childrenToc = childrenArray.find((x) => x.nodeName === 'UL');
		if(childrenToc) {
			createClickFunctionsOnElement(childrenToc.parentElement);
		}

	});
}