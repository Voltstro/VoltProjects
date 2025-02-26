/*
 * Main JS for admin side of Volt Projects
 */

import '../shared/index';
import { cronValidator } from '../shared/validation/cronValidator';
import { ValidationService } from '../shared/validation/validationService';
import { addCronExplainer } from './cron/cronExplain';

//Install validation service
const v = new ValidationService();
v.installValidator(cronValidator);
v.init();

const vAdmin = {
    addCronExplainer: addCronExplainer
};

declare global {
    interface Window { vAdmin: typeof vAdmin; }
}

//Provide globals
window.vAdmin = vAdmin;
