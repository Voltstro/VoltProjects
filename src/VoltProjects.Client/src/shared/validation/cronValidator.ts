import { ValidationService } from 'aspnet-client-validation';
import { isValidCron } from 'cron-validator';
import { ValidatorBase } from './validatorBase';

export const cronValidator: ValidatorBase = {
    install: function (validationService: ValidationService): void {
        validationService.addProvider('cron', (value) => {
            if (!value) {
                return true;
            }

            return isValidCron(value, {
                seconds: true,
            });
        });
    }
};
