import { Dropdown } from 'bootstrap';

export function initDropdowns(): void {
    const dropdownElementList = document.querySelectorAll('.dropdown-toggle')
    const dropdownList = Array.from(dropdownElementList).map(dropdownToggleEl => new Dropdown(dropdownToggleEl))
}
