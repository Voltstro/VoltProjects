import { Popover } from 'bootstrap';
import { debounce } from '../shared/utility';

export function initSearchNav(): void {
    //Attempt to get search nav
    const searchNavElement = document.getElementById('search-nav') as HTMLInputElement;
    if (!searchNavElement) return;

    //Get project and version IDs
    const projectId = document.querySelector<HTMLMetaElement>('meta[name="vpProjectId"]').content;
    const projectVersionId = document.querySelector<HTMLMetaElement>('meta[name="vpProjectVersionId"]').content;

    const project = `${projectId}|${projectVersionId}`;

    //Create Bootstrap popover
    const popover = new Popover(searchNavElement, {
        template: '<div class="popover popover-search" role="tooltip">' +
        '<div class="popover-arrow"></div>' +
        '<div class="popover-body" id="popover-body"></div>' +
        '</div>',
        placement: 'bottom',
        trigger: 'focus',
        content: 'Start typing to search...',
        html: true,
        sanitize: false
    });

    //Do query on input
    searchNavElement.oninput = debounce(async () => {
        const queryParams = new URLSearchParams(
            { 
                query: searchNavElement.value,
                size: '5',
                project: project
            }
        );

        //Do request
        const response = await fetch('/search/?' + queryParams, {
            method: 'POST'
        });

        //Set popover content to response payload
        const responsePayload = await response.text();
        popover.setContent({
            '.popover-body': responsePayload
        });
    });
}