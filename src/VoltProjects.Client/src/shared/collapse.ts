import { Collapse } from 'bootstrap';

const collapseElementList = document.querySelectorAll('.collapse');
Array.from(collapseElementList).map(collapseEl => new Collapse(collapseEl, { toggle: false }));
