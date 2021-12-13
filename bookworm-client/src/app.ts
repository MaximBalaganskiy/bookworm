import { autoinject } from "aurelia-framework";
import { Author } from "models/author";
import { Api } from "services/api";
import { MdcDialogService } from '@aurelia-mdc-web/dialog';
import { MdcSnackbarService } from '@aurelia-mdc-web/snackbar';
import { Rating } from "models/rating";
import { AddRatingDialog } from "add-rating-dialog/add-rating-dialog";


@autoinject()
export class App {
    constructor(private api: Api, private dialogService: MdcDialogService, private snackbarService: MdcSnackbarService) { }

    authors: Author[];

    attached() {
        this.refresh();
    }

    async refresh() {
        this.authors = await this.api.topAuthors(10);
    }

    async add() {
        const rating = new Rating();
        const res = await this.dialogService.open({ viewModel: AddRatingDialog, model: rating });
        if (res !== 'add') {
            return;
        }
        await this.api.addRating(rating);
        this.snackbarService.open('Thank you for the review! We will process it in a few seconds.', 'Ok', { timeout: 9999 })
    }
}


