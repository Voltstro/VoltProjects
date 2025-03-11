//Shared main entry
import '../shared';

import { renderAside } from './nav';
import { initToc } from './toc';
import { initSearchNav } from './searchNav';

/**
 * Main entry for public
 */
renderAside();
initToc();
initSearchNav();
