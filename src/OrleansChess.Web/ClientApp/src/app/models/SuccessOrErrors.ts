export interface ISuccessOrErrors<T> {
    data: T;
    wasSuccessful: boolean;
    errors: string[];
}

export class SuccessOrErrors<T> implements ISuccessOrErrors<T> {
    data: T;
    wasSuccessful: boolean;
    errors: string[];
}