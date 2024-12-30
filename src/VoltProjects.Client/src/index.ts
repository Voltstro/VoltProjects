/*
 * Main shared logic with both admin and main
 */

import { getPreferredTheme, setTheme, toggleTheme } from './theme';

declare global {
    interface Window { vGlobal: any; vAdmin: any }
}


//Main styling
import './scss/main.scss';

//Bootstrap JS stuff we use
import 'bootstrap/js/dist/collapse';
import 'bootstrap/js/dist/dropdown';
import 'bootstrap/js/dist/alert';

window.vGlobal = {
    toggleTheme: toggleTheme
};

setTheme(getPreferredTheme());
