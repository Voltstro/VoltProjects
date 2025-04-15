import { ValidationService as ClientValidator } from 'aspnet-client-validation';
import { ValidatorBase } from './validatorBase';


export class ValidationService {
    private readonly clientValidation: ClientValidator;

    constructor() {
        this.clientValidation = new ClientValidator();
        this.clientValidation.ValidationInputCssClassName = 'is-invalid';
        //this.clientValidation.ValidationInputValidCssClassName = 'is-valid';
        this.clientValidation.ValidationMessageCssClassName = 'invalid-feedback';
        //this.clientValidation.ValidationMessageValidCssClassName = 'valid-feedback';
    }

    public init(): void {
        this.clientValidation.bootstrap();
    }

    public installValidator(provider: ValidatorBase): void {
        provider.install(this.clientValidation);
    }
}
