import './collapse';
import './dropdown';
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
