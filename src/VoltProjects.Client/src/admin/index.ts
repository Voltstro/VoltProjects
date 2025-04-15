/*
 * Main JS for admin side of Volt Projects
 */

import 'bootstrap/js/dist/alert';

import '../shared';
import { cronValidator } from '../shared/validation/cronValidator';
import { ValidationService } from '../shared/validation/validationService';
import { addCronExplainer } from './cron/cronExplain';
import { initPreBuildCommandsHandler } from './prebuildCommands';

//Install validation service
const v = new ValidationService();
v.installValidator(cronValidator);
v.init();

const vAdmin = {
    addCronExplainer: addCronExplainer,

    initPreCommandHandler: initPreBuildCommandsHandler
};

declare global {
    interface Window { vAdmin: typeof vAdmin; }
}

//Provide globals
window.vAdmin = vAdmin;
