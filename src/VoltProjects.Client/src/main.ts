import { getPreferredTheme, setTheme, toggleTheme } from './theme';
import { renderAside } from './nav';
import { initToc } from './toc';
import { initSearchNav } from './searchNav';

//Main styling
import './scss/main.scss';

//Bootstrap JS stuff we use
import 'bootstrap/js/dist/collapse';
import 'bootstrap/js/dist/dropdown';

declare global {
    interface Window { vGlobal: any; }
}

window.vGlobal = {
    toggleTheme: toggleTheme
};

setTheme(getPreferredTheme());
renderAside();
initToc();
initSearchNav();
