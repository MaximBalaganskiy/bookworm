import { PLATFORM } from "aurelia-pal";
import { useView } from "aurelia-templating";
import { Rule, ValidationController, ValidationControllerFactory, ValidationRules } from "aurelia-validation";
import { Rating } from "models/rating";
import { MdcDialog } from '@aurelia-mdc-web/dialog';

@useView(PLATFORM.moduleName('add-rating-dialog/add-rating-dialog.html'))
export class AddRatingDialog {
    constructor(vcf: ValidationControllerFactory) {
        this.validationController = vcf.createForCurrentScope();
        this.rules = ValidationRules
            .ensure<Rating, string>(x => x.author).required()
            .ensure(x => x.title).required()
            .rules;
    }

    validationController: ValidationController;
    rules: Rule<Rating, unknown>[][];
    rating: Rating;
    dialog: MdcDialog;

    activate(rating: Rating) {
        this.rating = rating;
        this.validationController.addObject(this.rating, this.rules);
    }

    async add() {
        if (!(await this.validationController.validate()).valid) {
            return;
        }
        this.dialog.close('add');
    }
}