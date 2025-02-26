//Bootstrap JS stuff we use
import 'bootstrap/js/dist/collapse';
import 'bootstrap/js/dist/dropdown';
import 'bootstrap/js/dist/alert';

import { setTheme, getPreferredTheme, toggleTheme } from './theme';

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

setTheme(getPreferredTheme());
