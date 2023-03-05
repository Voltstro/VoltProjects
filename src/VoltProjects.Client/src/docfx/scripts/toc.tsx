export function renderToc(): void {
	const tocElement = document.getElementById('toc');
	if (!tocElement) {
		return;
	}

	createClickFunctionsOnElement(tocElement);
	registerTocEvents();
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
			});
		}

		const childrenToc = childrenArray.find((x) => x.nodeName === 'UL');
		if(childrenToc) {
			createClickFunctionsOnElement(childrenToc.parentElement);
		}

	});
}

function registerTocEvents(): void {
	const tocFilter = document.getElementById('toc-filter') as HTMLInputElement;
	if (!tocFilter) {
		return;
	}

	tocFilter.addEventListener('input', () => onTocFilterTextChange());

	// Set toc filter from local session storage on page load
	const filterString = sessionStorage?.filterString;
	if (filterString) {
		tocFilter.value = filterString;
		onTocFilterTextChange();
	}

	function onTocFilterTextChange(): void {
		const filter = tocFilter.value?.toLocaleLowerCase() || '';
		if (sessionStorage) {
			sessionStorage.filterString = filter;
		}

		const toc = document.getElementById('toc');
		const anchors = toc.querySelectorAll('a');

		if (filter == '') {
			anchors.forEach(a => a.parentElement.classList.remove('filtered', 'hide'));
			return;
		}

		const filteredLI = new Set<HTMLElement>();
		anchors.forEach(a => {
			const text = a.innerText;
			if (text && text.toLowerCase().indexOf(filter) >= 0) {
				let e: HTMLElement = a;
				while (e && e !== toc) {
					e = e.parentElement;
					filteredLI.add(e);
				}
			}
		});

		anchors.forEach(a => {
			const li = a.parentElement;
			if (filteredLI.has(li)) {
				li.classList.remove('hide');
				li.classList.add('filtered');
			} else {
				li.classList.add('hide');
			}
		});
	}
}