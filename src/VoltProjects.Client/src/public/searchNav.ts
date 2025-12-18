import { Modal } from 'bootstrap';
import searchModal from './components/searchModal';

export function initSearchNav(): void {
    //Attempt to get search nav
    const searchNavElement = document.getElementById('search-nav') as HTMLInputElement;
    if (!searchNavElement) return;

    //Get project and version IDs
    const projectId = document.querySelector<HTMLMetaElement>('meta[name="vpProjectId"]').content;
    const projectVersionId = document.querySelector<HTMLMetaElement>('meta[name="vpProjectVersionId"]').content;

    const project = `${projectId}|${projectVersionId}`;

    const modalElement = searchModal(project);
    const modal = new Modal(modalElement);

    function showSearchModal(event: Event, onInput: boolean): void {
        const target = event.target as HTMLInputElement;
        event.preventDefault();
        modal.show();

        const searchQuery = document.getElementById('searchQuery') as HTMLInputElement;
        searchQuery.focus();

        if (onInput) {
            searchQuery.value = target.value;
            target.value = '';
        }
    }

    searchNavElement.oninput = (event) => showSearchModal(event, true);
    searchNavElement.onclick = (event) => showSearchModal(event, false);
}