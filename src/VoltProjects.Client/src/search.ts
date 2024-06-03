/**
 * Sets up the search form's query input
 */
function setupSearchInput(searchForm: HTMLFormElement): void {
	const searchInput = document.getElementById('search') as HTMLInputElement;
	searchForm.addEventListener('submit', event => {
		if(!searchForm.checkValidity()) {
			event.preventDefault();
			event.stopPropagation();
	
			searchInput.classList.add('is-invalid');
			searchInput.addEventListener('input', updateInputClass);
		}
	}, false);

	function updateInputClass(): void {
		searchInput.classList.remove('is-invalid');
		searchInput.removeEventListener('input', updateInputClass);
	}	
}

/**
 * Sets up the search form's project checkboxes
 */
function setupSearchProjectCheckboxes(searchForm: HTMLFormElement): void {
	const projects = searchForm.querySelectorAll<HTMLLIElement>('li[id^="project-"]');
	projects.forEach(x => {
		//Get main project checkbox
		const projectCheck = x.querySelector<HTMLInputElement>('input[id^="project-check"]');

		//Get children checkboxes
		const childProjectChecks = x.querySelectorAll<HTMLInputElement>('input[id^="project-version-check"]');

		//Update inputs on initial load
		updateInput(projectCheck, childProjectChecks);
	
		//Add event listener
		projectCheck.addEventListener('change', () => {
		//Update each check change
			updateInput(projectCheck, childProjectChecks);
		});
	});

	function updateInput(input: HTMLInputElement, childInputs: NodeListOf<HTMLInputElement>): void {
		const checked = input.checked;

		childInputs.forEach(x => x.disabled = !checked);

		if(checked) {
			if(childInputs.length == 1) {
				childInputs[0].setAttribute('data-readonly', 'true');
				childInputs[0].checked = true;
				childInputs[0].addEventListener('click', preventClick);
			}
		} else {
			if(childInputs.length == 1) {
				childInputs[0].removeEventListener('click', preventClick);
			}
		}
	}

	function preventClick(event: MouseEvent): void {
		event.preventDefault();
	}
}

document.addEventListener('DOMContentLoaded', () => {
	const searchForm = document.getElementById('search-form') as HTMLFormElement;
	setupSearchInput(searchForm);
	setupSearchProjectCheckboxes(searchForm);
});
