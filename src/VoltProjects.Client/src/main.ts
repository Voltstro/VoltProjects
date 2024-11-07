/*
 * Main JS for public side of Volt Projects
 */

import './index';
import { renderAside } from './nav';
import { initToc } from './toc';
import { initSearchNav } from './searchNav';

renderAside();
initToc();
initSearchNav();
