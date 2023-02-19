// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { meta } from './utility';

// Styling for tables in conceptual documents using Bootstrap.
// See http://getbootstrap.com/css/#tables
export function renderTables(): void {
	document.querySelectorAll('table').forEach(table => {
		table.classList.add('table', 'table-bordered');
		const wrapper = document.createElement('div');
		wrapper.className = 'table-responsive';
		table.parentElement.insertBefore(wrapper, table);
		wrapper.appendChild(table);
	});
}

// Styling for alerts.
export function renderAlerts(): void {
	document.querySelectorAll('.NOTE').forEach(e => e.classList.add('alert'));
	document.querySelectorAll('.TIP').forEach(e => e.classList.add('alert'));
	document.querySelectorAll('.CAUTION').forEach(e => e.classList.add('alert'));
	document.querySelectorAll('.WARNING').forEach(e => e.classList.add('alert'));
	document.querySelectorAll('.IMPORTANT').forEach(e => e.classList.add('alert'));
}

// Open external links to different host in a new window.
export function renderLinks(): void {
	if (meta('docfx:newtab') === 'true') {
		const links = document.links;
		for (let i = 0; i < links.length; i++) {
			const link = links.item(i);
			if (link.hostname !== window.location.hostname) {
				link.target = '_blank';
			}
		}
	}
}