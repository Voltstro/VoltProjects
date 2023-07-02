import { enableAnchor } from './scripts/anchor';
import { highlight } from './scripts/highlight';
import { renderAside } from './scripts/nav';
import { renderToc } from './scripts/toc';

//Import styling
import './scss/projectPage/_main.scss';

document.addEventListener('DOMContentLoaded', onContentLoad);

function onContentLoad(): void {
	void enableAnchor();
	void highlight();

	renderAside();
	renderToc();
}