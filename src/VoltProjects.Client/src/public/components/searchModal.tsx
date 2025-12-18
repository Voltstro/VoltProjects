

import * as React from 'jsx-dom';
import { debounce } from '../../shared/utility';

export default (project: string) => {
    const onInput = debounce(async () => {
        const value = (document.getElementById('searchQuery') as HTMLInputElement).value;

        const queryParams = new URLSearchParams(
        { 
            query: value,
            size: '5',
            project: project
        });

        //Do request
        const response = await fetch('/search/?' + queryParams, {
            method: 'POST'
        });

        const searchResults = document.getElementById('searchResults');
        const responsePayload = await response.text();
        searchResults.innerHTML = responsePayload;
    });

    return (
        <div class='modal'>
            <div class='modal-dialog'>
                <div class='modal-content'>
                    <div class='modal-header'>
                        <h1 class="modal-title fs-5">Search</h1>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class='modal-body'>
                        <form method='get' action='/search/'>
                            <input type='hidden' name='project' value={project}/>
                            <div class='pb-4'>
                                <input class='form-control' type='text' id='searchQuery' name='query' placeholder='Search...' autoComplete='off' onInput={onInput}></input>
                            </div>
                            <div id='searchResults'>
                                <span class='text-secondary'>Start typing to search...</span>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    )
}