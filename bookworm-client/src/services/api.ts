import { Author } from "models/author";
import { HttpClient } from 'aurelia-fetch-client';
import { autoinject } from "aurelia-dependency-injection";
import { IEnvironment } from '../models/i-environment';
import { Rating } from "models/rating";

@autoinject
export class Api {
    constructor(private httpClient: HttpClient, private environment: IEnvironment) { }

    async topAuthors(count: number): Promise<Author[]> {
        var response = await this.httpClient.fetch(`${this.environment.topAuthorsUrl}&count=${count}`);
        return await response.json() as Author[];
    }

    async addRating(rating: Rating): Promise<void> {
        await this.httpClient.post(this.environment.addRatingUrl, JSON.stringify(rating));
    }
}