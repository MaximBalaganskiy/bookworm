import { HttpClient } from 'aurelia-fetch-client';
import { Aurelia } from 'aurelia-framework';
import { PLATFORM } from 'aurelia-pal';
import { IEnvironment } from './models/i-environment';

export async function configure(aurelia: Aurelia) {

    aurelia.use
        .standardConfiguration()
        .plugin(PLATFORM.moduleName('@aurelia-mdc-web/all'))
        .plugin(PLATFORM.moduleName('aurelia-validation'))
        .globalResources([]);

    aurelia.use.developmentLogging("debug");

    var httpClient = aurelia.container.get(HttpClient);
    var env = await httpClient.fetch('./environment.json').then(x => x.json());
    aurelia.container.registerInstance(IEnvironment, env);

    aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app')));
}
