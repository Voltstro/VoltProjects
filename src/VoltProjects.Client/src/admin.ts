/*
 * Main JS for admin side of Volt Projects
 */

import './index';
import { cronValidator } from './validation/cronValidator';
import { ValidationService } from './validation/validationService';
import { addCronExplainer } from './admin/cron/cronExplain';

//Install validation service
const v = new ValidationService();
v.installValidator(cronValidator);
v.init();

//Provide globals
window.vAdmin = {
    addCronExplainer: addCronExplainer
};
