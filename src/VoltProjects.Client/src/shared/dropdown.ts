import { Dropdown } from 'bootstrap';

const dropdownElementList = document.querySelectorAll('.dropdown-toggle');
Array.from(dropdownElementList).map(dropdownToggleEl => new Dropdown(dropdownToggleEl));