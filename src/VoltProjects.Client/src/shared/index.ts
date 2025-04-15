//Bootstrap JS stuff we use
import 'bootstrap/js/dist/collapse';

import { setTheme, getPreferredTheme, toggleTheme } from './theme';
import { initDropdowns } from './dropdown';

/**
 * Main shared entry point
 */

const vGlobal = {
    toggleTheme: toggleTheme
};

declare global {
    interface Window { vGlobal: typeof vGlobal; }
}

window.vGlobal = vGlobal;

initDropdowns();
setTheme(getPreferredTheme());
