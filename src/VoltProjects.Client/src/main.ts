import { getPreferredTheme, setTheme } from './theme';
import { renderAside } from './scripts/nav';
import { renderToc } from './scripts/toc';

//Force include Bootstrap icon fonts
import 'bootstrap-icons/font/fonts/bootstrap-icons.woff';
import 'bootstrap-icons/font/fonts/bootstrap-icons.woff2';

//Main styling
import './scss/main.scss';

//Bootstrap collapse, used by navbar, and as such every page
import 'bootstrap/js/dist/collapse';
import 'bootstrap/js/dist/dropdown';

declare global {
    interface Window { vGlobal: any; }
}

document.addEventListener('DOMContentLoaded', onContentLoad);

function onContentLoad(): void {
	setTheme(getPreferredTheme());

	renderAside();
	renderToc();
}

window.vGlobal = {
	setTheme: setTheme
};
