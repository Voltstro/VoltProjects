import Offcanvas from 'bootstrap/js/dist/offcanvas';
import React from 'jsx-dom';

function setupNavbarCollapse(): void {

	const collapseElementList = document.querySelectorAll('[data-merge-nav="true"');
	
	const mergedElements = Array.from(collapseElementList)
		.flatMap((x) => Array.from(x.childNodes).map((y) => y.cloneNode(true)));

	const navElement = document.getElementsByTagName('nav');
	const newNav = navElement[0].appendChild(
		<div class="offcanvas offcanvas-end" aria-labelledby="offcanvasnavlabel" tabIndex={-1}>
			<div class="offcanvas-header">
				<h5 class="offcanvas-title" id="offcanvasnavlabel">VoltProjects</h5>
				<button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Close"></button>
			</div>
			<div class="offcanvas-body">
				<div class="dropdown mt-3" id="offcanvasmenu">
					{mergedElements}
				</div>
			</div>
		</div>);

	const offcanvas = new Offcanvas(newNav);
	let toggle = false;
	document.getElementById('navbarToggle').addEventListener('click', () => {
		console.log('hi');
		if(toggle) {
			offcanvas.hide();
			toggle = true;
		} else {
			offcanvas.show();
			toggle = false;
		}

	});
}

document.addEventListener('DOMContentLoaded', setupNavbarCollapse);
