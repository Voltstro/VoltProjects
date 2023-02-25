import './scss/docfx.scss';

import 'bootstrap/js/dist/collapse';
import { enableAnchor } from './scripts/anchor';
import { highlight } from './scripts/highlight';
import { renderAlerts, renderLinks, renderTables } from './scripts/markdown';
import { renderAside } from './scripts/nav';

document.addEventListener('DOMContentLoaded', onContentLoad);

function onContentLoad(): void {
	enableAnchor();
	highlight();

	renderAside();
	renderAlerts();
	renderLinks();
	renderTables();
}