import { ValidationService } from 'aspnet-client-validation';

export interface ValidatorBase {
    install(validationService: ValidationService): void;
}
