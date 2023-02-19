import './scss/docfx.scss';

import { enableAnchor } from './scripts/anchor';
import { highlight } from './scripts/highlight';
import { renderAlerts, renderLinks, renderTables } from './scripts/markdown';

document.addEventListener('DOMContentLoaded', onContentLoad);

function onContentLoad(): void {
	enableAnchor();
	highlight();

	renderAlerts();
	renderLinks();
	renderTables();
}