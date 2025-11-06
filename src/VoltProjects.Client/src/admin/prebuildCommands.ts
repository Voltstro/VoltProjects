import Sortable from 'sortablejs';

const PREBUILD_COMMAND_ID_PREFIX = 'PreBuildCommands';

export function initPreBuildCommandsHandler(): void {
    //Get child elements of list
    const childPreBuildCommandsListElements = getPreBuildCommandElements();
    let preBuildCommandsCount = childPreBuildCommandsListElements.length;

    //Install sortable JS onto the list
    const preBuildCommandsList = document.getElementById(PREBUILD_COMMAND_ID_PREFIX) as HTMLUListElement;
    Sortable.create(preBuildCommandsList, {
        handle: '.admin-dragable-handle',
        onChange: () => {
            const childPreBuildCommandsListElements = getPreBuildCommandElements();
            for (let i = 0; i < preBuildCommandsCount; i++) {
                const element = childPreBuildCommandsListElements[i];
                const itemOrderElement = element.querySelector('[data-pbc-order]') as HTMLInputElement;
                itemOrderElement.value = i.toString();
            }
        }
    });

    //Install add pre-build command button handler
    const preBuildCommandsButton = document.getElementById(`${PREBUILD_COMMAND_ID_PREFIX}Btn`) as HTMLButtonElement;
    preBuildCommandsButton.addEventListener('click', () => {
        const listItem = document.createElement('li');
        listItem.setAttribute('class', 'list-group-item list-group-item-success');

        //New item
        const newInput = document.createElement('input');
        newInput.setAttribute('type', 'hidden');
        newInput.setAttribute('id', `${PREBUILD_COMMAND_ID_PREFIX}_${preBuildCommandsCount}__New`);
        newInput.setAttribute('name', `${PREBUILD_COMMAND_ID_PREFIX}[${preBuildCommandsCount}].New`);
        newInput.value = 'True';
        listItem.appendChild(newInput);

        //Order input
        const orderInput = document.createElement('input');
        orderInput.setAttribute('type', 'hidden');
        orderInput.setAttribute('id', `${PREBUILD_COMMAND_ID_PREFIX}_${preBuildCommandsCount}__Order`);
        orderInput.setAttribute('name', `${PREBUILD_COMMAND_ID_PREFIX}[${preBuildCommandsCount}].Order`);
        orderInput.setAttribute('data-pbc-order', '');
        orderInput.value = preBuildCommandsCount.toString();
        listItem.appendChild(orderInput);
        
        const listItemRow = document.createElement('div');
        listItemRow.setAttribute('class', 'row');

        //Drag handle
        const dragHandleDiv = document.createElement('div');
        dragHandleDiv.setAttribute('class', 'col-auto align-content-center p-0 ps-1 admin-dragable-handle');

        const dragHandle = document.createElement('i');
        dragHandle.setAttribute('class', 'bi bi-grip-vertical');
        dragHandleDiv.appendChild(dragHandle);
        listItemRow.appendChild(dragHandleDiv);

        //Command
        const commandDiv = document.createElement('div');
        commandDiv.setAttribute('class', 'col-2');

        const commandInput = document.createElement('input');
        commandInput.setAttribute('class', 'form-control');
        commandInput.setAttribute('id', `${PREBUILD_COMMAND_ID_PREFIX}_${preBuildCommandsCount}__Command`);
        commandInput.setAttribute('name', `${PREBUILD_COMMAND_ID_PREFIX}[${preBuildCommandsCount}].Command`);
        commandInput.setAttribute('type', 'text');
        commandInput.setAttribute('placeholder', 'Command...');
        commandInput.setAttribute('data-val', 'true');
        commandInput.setAttribute('data-val-required', 'The Command field is required.');
        commandDiv.appendChild(commandInput);
        listItemRow.appendChild(commandDiv);

        //Arguments
        const argumentsDiv = document.createElement('div');
        argumentsDiv.setAttribute('class', 'col');

        const argumentInput = document.createElement('input');
        argumentInput.setAttribute('class', 'form-control');
        argumentInput.setAttribute('id', `${PREBUILD_COMMAND_ID_PREFIX}_${preBuildCommandsCount}__Arguments`);
        argumentInput.setAttribute('name', `${PREBUILD_COMMAND_ID_PREFIX}[${preBuildCommandsCount}].Arguments`);
        argumentInput.setAttribute('type', 'text');
        argumentInput.setAttribute('placeholder', 'Arguments...');
        argumentsDiv.appendChild(argumentInput);
        listItemRow.appendChild(argumentsDiv);

        listItem.appendChild(listItemRow);

        preBuildCommandsList.appendChild(listItem);

        //Increment counter
        preBuildCommandsCount++;
    });

    //Install delete button handler
    for (let i = 0; i < preBuildCommandsCount; i++) {
        const element = childPreBuildCommandsListElements[i] as HTMLLIElement;
        const deleteBtn = element.querySelector('[data-pbc-deletebtn]') as HTMLButtonElement;
        deleteBtn.addEventListener('click', () => {
            const deletedInputElement = element.querySelector(`[id='${PREBUILD_COMMAND_ID_PREFIX}_${i}__Deleted']`) as HTMLInputElement;
            deletedInputElement.value = 'true';

            element.classList.add('list-group-item-danger');

            deleteBtn.disabled = true;
        });
    }
}

function getPreBuildCommandElements(): Element[] {
    const preBuildCommandsList = document.getElementById(PREBUILD_COMMAND_ID_PREFIX) as HTMLUListElement;
    return Array.from(preBuildCommandsList.children);;
}
