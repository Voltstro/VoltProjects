import { isValidCron } from 'cron-validator';
import { toString } from 'cronstrue';

function explainCron(input: string): string {
    if(!isValidCron(input, {
        seconds: true
    })) {
        return '';
    }

    return toString(input);
}

export function addCronExplainer(input: HTMLInputElement, explainResult: HTMLElement): void {
    if(!input || !explainResult) {
        throw new Error('Inputs must be valid!');
    }

    explainResult.innerText = explainCron(input.value);
    input.addEventListener('input', () => {
        explainResult.innerText = explainCron(input.value);
    });
}
