import { platformBrowser } from '@angular/platform-browser';
import { registerLocaleData } from '@angular/common';
import localeEs from '@angular/common/locales/es';
import { AppModule } from './app/app-module';

registerLocaleData(localeEs);

platformBrowser().bootstrapModule(AppModule, {

})
  .catch(err => console.error(err));
