import { ValidationService } from 'aspnet-client-validation';

/*
 * Validation service, allows nicer inputs
 */

const v = new ValidationService();
v.ValidationInputCssClassName = 'is-invalid';
v.ValidationInputValidCssClassName = 'is-valid';
v.ValidationMessageCssClassName = 'invalid-feedback';
v.ValidationMessageValidCssClassName = 'valid-feedback';
v.bootstrap();
